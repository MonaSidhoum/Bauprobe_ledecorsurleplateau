using Bauprobe.Core;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace Bauprobe.Calibration
{
    /// <summary>
    /// Cale le repère du plateau sur le QR code posé sur la scène.
    ///
    /// Ce script écoute directement MRUK (le kit de Meta qui détecte les QR codes) :
    /// on n'a donc plus besoin de modifier les scripts de l'exemple Meta.
    /// (Anciennement "QRSceneCoordinator".)
    /// </summary>
    public class StageCalibrator : MonoBehaviour
    {
        [Header("Repère du décor")]
        [Tooltip("Objet déplacé sur le QR code. Il contient StageRoot.")]
        [SerializeField] private Transform qrAnchor;

        [Tooltip("Objet qui contient tout le décor (avec le composant StageSpace).")]
        [SerializeField] private Transform stageRoot;

        [Header("Hauteur du QR code")]
        [Tooltip("Réglages du plateau (distance entre le bas du QR code et le sol). " +
                 "Vide = le fichier Assets/_Bauprobe/Core/Resources/StageSettings.asset.")]
        [SerializeField] private StageSettings settings;

        [Header("Réglages")]
        [Tooltip("Texte contenu dans le QR code du plateau. Vide = accepte n'importe quel QR code.")]
        [SerializeField] private string expectedPayload = "";

        [Tooltip("Si coché, le décor ne bouge plus après la première détection (évite les petits sauts). " +
                 "Appeler Unlock() pour recaler.")]
        [SerializeField] private bool lockAfterFirstDetection = true;

        [Tooltip("Garde le repère du plateau horizontal : seule l'orientation (le cap) du QR code est utilisée. " +
                 "Indispensable si le QR code est posé à plat sur le sol.")]
        [SerializeField] private bool keepLevel = true;

        private MRUKTrackable _trackedQr;
        private StageSpace _stageSpace;

        /// <summary>Vrai tant que le casque voit le QR code du plateau.</summary>
        public bool IsQrLocated => _trackedQr != null;

        /// <summary>Vrai si la calibration est figée (le décor ne suit plus le QR code).</summary>
        public bool IsLocked { get; private set; }

        private void Start()
        {
            if (settings == null)
            {
                settings = StageSettings.LoadDefault();
                if (settings == null)
                {
                    Debug.LogWarning("StageSettings introuvable : on considère que le QR code est posé au sol.");
                }
            }

            if (stageRoot != null)
            {
                _stageSpace = stageRoot.GetComponent<StageSpace>();
            }

            if (!MRUK.Instance)
            {
                Debug.LogError($"{nameof(StageCalibrator)} a besoin d'un objet MRUK dans la scène.");
                return;
            }

            MRUK.Instance.SceneSettings.TrackableAdded.AddListener(OnTrackableAdded);
            MRUK.Instance.SceneSettings.TrackableRemoved.AddListener(OnTrackableRemoved);
        }

        private void OnDestroy()
        {
            if (MRUK.Instance)
            {
                MRUK.Instance.SceneSettings.TrackableAdded.RemoveListener(OnTrackableAdded);
                MRUK.Instance.SceneSettings.TrackableRemoved.RemoveListener(OnTrackableRemoved);
            }
        }

        private void Update()
        {
            // MRUK affine la position du QR code au fil du temps : on la suit
            // tant que la calibration n'est pas figée.
            if (_trackedQr != null && !IsLocked)
            {
                AlignToQr();
            }
        }

        /// <summary>Fige le décor à sa position actuelle (à brancher sur un bouton).</summary>
        public void Lock() => IsLocked = true;

        /// <summary>Le décor suit de nouveau le QR code (à brancher sur un bouton).</summary>
        public void Unlock() => IsLocked = false;

        private void OnTrackableAdded(MRUKTrackable trackable)
        {
            if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode || !MatchesPayload(trackable))
            {
                return;
            }

            _trackedQr = trackable;
            if (!IsLocked)
            {
                AlignToQr();
            }
        }

        private void OnTrackableRemoved(MRUKTrackable trackable)
        {
            if (trackable != _trackedQr)
            {
                return;
            }

            // On ne détruit pas le décor : il reste là où il était.
            _trackedQr = null;
            Debug.Log("QR perdu : décor conservé.");
        }

        private bool MatchesPayload(MRUKTrackable trackable)
        {
            return string.IsNullOrEmpty(expectedPayload) || trackable.MarkerPayloadString == expectedPayload;
        }

        /// <summary>
        /// Rotation horizontale (seulement le cap) tirée du QR code, qu'il soit posé
        /// au sol ou accroché à un mur : on prend l'un de ses axes qui est à plat.
        /// </summary>
        private static Quaternion LevelRotation(Transform qr)
        {
            var forward = Vector3.ProjectOnPlane(qr.up, Vector3.up);
            if (forward.sqrMagnitude < 0.01f)
            {
                forward = Vector3.ProjectOnPlane(qr.forward, Vector3.up);
            }

            return Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        /// <summary>Hauteur (repère du monde) du bord le plus bas du QR code.</summary>
        private static float QrBottomY(MRUKTrackable trackable)
        {
            var qr = trackable.transform;
            if (trackable.PlaneRect is not { } rect)
            {
                return qr.position.y; // taille inconnue : on prend le centre
            }

            // Les 4 coins du QR code, dans son plan (X, Y local) : on garde le plus bas.
            var lowest = float.MaxValue;
            foreach (var corner in new[]
                     {
                         new Vector3(rect.xMin, rect.yMin, 0f), new Vector3(rect.xMax, rect.yMin, 0f),
                         new Vector3(rect.xMin, rect.yMax, 0f), new Vector3(rect.xMax, rect.yMax, 0f),
                     })
            {
                lowest = Mathf.Min(lowest, qr.TransformPoint(corner).y);
            }
            return lowest;
        }

        private void AlignToQr()
        {
            var qr = _trackedQr.transform;

            // Le repère du plateau est posé AU SOL, à la verticale du centre du QR code :
            // sol = bas du QR code - distance mesurée entre le bas du QR code et le sol (StageSettings).
            var qrBottomY = QrBottomY(_trackedQr);
            var floorY = settings != null ? settings.FloorHeight(qrBottomY) : qrBottomY;
            var anchorPosition = new Vector3(qr.position.x, floorY, qr.position.z);

            qrAnchor.SetPositionAndRotation(anchorPosition, keepLevel ? LevelRotation(qr) : qr.rotation);
            Debug.Log($"Plateau calé : sol à {qrBottomY - floorY:0.00} m sous le bas du QR code.");

            if (lockAfterFirstDetection)
            {
                IsLocked = true;
                Debug.Log("QR détecté : décor recalé et figé.");
            }

            if (_stageSpace != null)
            {
                _stageSpace.NotifyCalibrated();
            }
        }
    }
}
