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

        private void AlignToQr()
        {
            var qr = _trackedQr.transform;
            qrAnchor.SetPositionAndRotation(qr.position, keepLevel ? LevelRotation(qr) : qr.rotation);

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
