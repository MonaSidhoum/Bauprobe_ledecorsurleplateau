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

        // Hauteur (repère du monde) du bas du QR code au dernier calage : pour replacer le sol
        // quand on change la hauteur du QR code dans le casque, même si le QR code n'est plus visible.
        private float? _lastQrBottomY;

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

            // Offset du QR code saisi dans le casque (écran « QR code offset ») : appliqué tout de suite,
            // puis à chaque changement.
            ApplyQrOffset(QrOffsetStore.Current);
            QrOffsetStore.Changed += ApplyQrOffset;

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
            QrOffsetStore.Changed -= ApplyQrOffset;

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
        /// Rotation horizontale (seulement le cap) tirée du QR code. L'« avant » du plateau (Y+ du tableau) est :
        ///   - QR code vertical (mur, pied) : la direction dans laquelle on regarde quand on le lit, du côté
        ///     imprimé vers l'arrière du QR code. Le côté est choisi d'après la position du casque, qui voit
        ///     forcément le côté imprimé : le résultat ne dépend pas des conventions de MRUK.
        ///     QR code lu depuis la salle → avant = lointain, droite = cour ;
        ///   - QR code à plat au sol : la direction de son bord du haut.
        /// On prend l'axe du QR code le plus proche de l'horizontale, pour qu'un QR code un peu penché
        /// ne fasse pas basculer le repère d'un demi-tour.
        /// </summary>
        private static Quaternion LevelRotation(Transform qr, Vector3 viewerPosition)
        {
            var top = Vector3.ProjectOnPlane(qr.up, Vector3.up);         // bord du haut
            var normal = Vector3.ProjectOnPlane(qr.forward, Vector3.up); // perpendiculaire au QR code

            Vector3 forward;
            if (normal.sqrMagnitude > top.sqrMagnitude)
            {
                // QR code plutôt vertical : sa perpendiculaire, dans le sens du regard de celui qui le lit.
                forward = normal;
                var viewerToQr = Vector3.ProjectOnPlane(qr.position - viewerPosition, Vector3.up);
                if (Vector3.Dot(forward, viewerToQr) < 0f)
                {
                    forward = -forward;
                }
            }
            else
            {
                forward = top; // QR code plutôt à plat
            }

            if (forward.sqrMagnitude < 1e-6f)
            {
                forward = Vector3.forward;
            }
            return Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        /// <summary>Position du casque (pour savoir de quel côté du QR code on se trouve).</summary>
        private static Vector3 ViewerPosition(Transform qr)
        {
            var cam = Camera.main;
            // Sans caméra : on se place « devant » le QR code au sens de MRUK.
            return cam != null ? cam.transform.position : qr.position + qr.forward;
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

        /// <summary>
        /// Applique l'offset du QR code saisi dans le casque :
        ///   - la hauteur du QR code replace le sol (QRAnchor) si le QR code a déjà été vu ;
        ///   - X / Y placent StageRoot (enfant de QRAnchor) au point zéro de la scène. Offset nul : StageRoot
        ///     reste sur le QR code.
        /// Le décor étant enfant de StageRoot, ses positions (sauvegarde JSON, tableau CSV) sont alors
        /// mesurées depuis le point zéro, et plus depuis le QR code : le décor déjà posé suit le point zéro.
        /// </summary>
        private void ApplyQrOffset(QrOffset offset)
        {
            if (offset == null)
            {
                return;
            }

            if (_lastQrBottomY is { } qrBottomY && qrAnchor != null)
            {
                var p = qrAnchor.position;
                qrAnchor.position = new Vector3(p.x, offset.FloorHeight(qrBottomY), p.z);
            }

            if (stageRoot == null)
            {
                return;
            }

            if (stageRoot.parent != qrAnchor)
            {
                Debug.LogWarning("StageRoot n'est pas enfant de QRAnchor : l'offset du QR code est ignoré.", stageRoot);
                return;
            }

            var reference = offset.ReferenceInQrFrame();
            stageRoot.SetLocalPositionAndRotation(reference.position, reference.rotation);
        }

        private void AlignToQr()
        {
            var qr = _trackedQr.transform;

            // Le repère du plateau est posé AU SOL, à la verticale du centre du QR code :
            // sol = bas du QR code - distance mesurée entre le bas du QR code et le sol (saisie dans le casque).
            var qrBottomY = QrBottomY(_trackedQr);
            _lastQrBottomY = qrBottomY;
            var floorY = QrOffsetStore.Current.FloorHeight(qrBottomY);
            var anchorPosition = new Vector3(qr.position.x, floorY, qr.position.z);

            qrAnchor.SetPositionAndRotation(anchorPosition, keepLevel ? LevelRotation(qr, ViewerPosition(qr)) : qr.rotation);
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
