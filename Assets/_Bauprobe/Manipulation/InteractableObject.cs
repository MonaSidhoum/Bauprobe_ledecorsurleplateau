using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// À mettre sur le cube (ou sur la racine de n'importe quel objet avec un Collider).
    /// Contient les 5 actions du menu : supprimer, couleur, déplacer, scale, rotation.
    ///
    /// Pendant Déplacer / Scale / Rotation :
    ///   - gâchette droite  = valider
    ///   - bouton B         = annuler (retour à l'état d'avant)
    ///   - joystick droit   = ajuster (voir chaque mode)
    /// </summary>
    public class InteractableObject : MonoBehaviour
    {
        public enum Mode { None, Move, Scale, Rotate }

        // --- état global : une seule manipulation à la fois ---
        public static InteractableObject Active { get; private set; }
        public static bool IsAnyManipulating => Active != null;
        static int s_lastEndFrame = -1;
        public static bool ManipulationEndedThisFrame => s_lastEndFrame == Time.frameCount;

        [SerializeField] OVRInput.Controller controller = OVRInput.Controller.RTouch;
        [Tooltip("Ancre de la manette droite. Laissé vide = RightHandAnchor de l'OVRCameraRig, trouvé automatiquement.")]
        [SerializeField] Transform controllerAnchor;
        [SerializeField, Range(0f, 0.5f)] float stickDeadZone = 0.15f;

        [Header("Couleur")]
        [SerializeField] Color[] palette =
        {
            Color.white, new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.75f, 0.3f),
            new Color(0.2f, 0.45f, 0.95f), new Color(1f, 0.8f, 0.1f), new Color(0.7f, 0.3f, 0.9f)
        };

        [Header("Déplacer")]
        [Tooltip("Vitesse (m/s) pour éloigner/rapprocher l'objet avec le joystick (haut/bas).")]
        [SerializeField] float pushPullSpeed = 1f;
        [SerializeField] float minGrabDistance = 0.1f;

        [Header("Scale")]
        [SerializeField] float scaleSpeed = 1f;      // facteur par seconde, joystick à fond
        [SerializeField] float minScale = 0.02f;     // en localScale.x
        [SerializeField] float maxScale = 5f;

        [Header("Rotation")]
        [SerializeField] float rotationSpeed = 90f;  // degrés par seconde

        public Mode CurrentMode { get; private set; } = Mode.None;

        Renderer[] _renderers;
        int _colorIndex;
        int _startFrame;

        // sauvegarde pour annuler
        Vector3 _savedPos, _savedScale;
        Quaternion _savedRot;

        // offset de l'objet dans le repère de la manette (pour Déplacer)
        Vector3 _grabLocalPos;

        void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();

            if (!controllerAnchor)
            {
                var rig = FindAnyObjectByType<OVRCameraRig>();
                if (rig) controllerAnchor = rig.rightHandAnchor;
            }
        }

        // ------------------------------------------------------------------
        // Actions appelées par le menu
        // ------------------------------------------------------------------

        public void Delete()
        {
            if (ObjectMenu.Instance) ObjectMenu.Instance.Hide();
            Destroy(gameObject);
        }

        public void ChangeColor()
        {
            if (palette == null || palette.Length == 0) return;
            _colorIndex = (_colorIndex + 1) % palette.Length;
            Color c = palette[_colorIndex];

            foreach (var r in _renderers)
            {
                foreach (var mat in r.materials) // .materials crée une instance propre à cet objet
                {
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c); // URP Lit
                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);         // Standard / Built-in
                }
            }
        }

        public void BeginManipulation(Mode mode)
        {
            if (mode == Mode.None || IsAnyManipulating) return;
            if (mode == Mode.Move && !controllerAnchor)
            {
                Debug.LogWarning($"{name}: pas d'ancre de manette, impossible de déplacer.", this);
                return;
            }

            _savedPos = transform.position;
            _savedRot = transform.rotation;
            _savedScale = transform.localScale;

            if (mode == Mode.Move)
                _grabLocalPos = controllerAnchor.InverseTransformPoint(transform.position);

            CurrentMode = mode;
            Active = this;
            _startFrame = Time.frameCount;

            // On cache le menu pendant la manipulation
            if (ObjectMenu.Instance) ObjectMenu.Instance.Hide();
        }

        // ------------------------------------------------------------------

        void Update()
        {
            if (CurrentMode == Mode.None) return;

            // Ne pas valider dans la frame même où la manipulation a commencé
            if (Time.frameCount != _startFrame)
            {
                if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller)) { EndManipulation(true); return; }
                if (OVRInput.GetDown(OVRInput.Button.Two, controller)) { EndManipulation(false); return; } // B
            }

            Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller);
            if (Mathf.Abs(stick.x) < stickDeadZone) stick.x = 0f;
            if (Mathf.Abs(stick.y) < stickDeadZone) stick.y = 0f;
            float dt = Time.deltaTime;

            switch (CurrentMode)
            {
                case Mode.Move:
                    // L'objet suit la manette ; joystick haut/bas = éloigner/rapprocher le long du rayon
                    _grabLocalPos.z = Mathf.Max(minGrabDistance, _grabLocalPos.z + stick.y * pushPullSpeed * dt);
                    transform.position = controllerAnchor.TransformPoint(_grabLocalPos);
                    break;

                case Mode.Scale:
                    // Joystick haut = agrandir, bas = rétrécir (uniforme)
                    if (stick.y != 0f)
                    {
                        float current = transform.localScale.x;
                        float target = Mathf.Clamp(current * (1f + stick.y * scaleSpeed * dt), minScale, maxScale);
                        transform.localScale *= target / current;
                    }
                    break;

                case Mode.Rotate:
                    // Joystick gauche/droite = tourner autour de la verticale.
                    // Pas de bascule avant/arrière : les objets de décor restent toujours droits.
                    transform.Rotate(Vector3.up, -stick.x * rotationSpeed * dt, Space.World);
                    break;
            }
        }

        void EndManipulation(bool confirm)
        {
            if (!confirm)
            {
                transform.SetPositionAndRotation(_savedPos, _savedRot);
                transform.localScale = _savedScale;
            }

            CurrentMode = Mode.None;
            Active = null;
            s_lastEndFrame = Time.frameCount;

            // On réaffiche le menu au-dessus de l'objet
            if (ObjectMenu.Instance) ObjectMenu.Instance.Show(this);
        }

        public Bounds GetWorldBounds()
        {
            bool has = false;
            var b = new Bounds(transform.position, Vector3.zero);
            foreach (var r in _renderers)
            {
                if (!r) continue;
                if (!has) { b = r.bounds; has = true; }
                else b.Encapsulate(r.bounds);
            }
            return b;
        }

        void OnDisable()
        {
            if (Active == this) { Active = null; s_lastEndFrame = Time.frameCount; }
            CurrentMode = Mode.None;
        }
    }
}
