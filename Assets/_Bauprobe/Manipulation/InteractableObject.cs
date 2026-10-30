using Bauprobe.Decor;
using UnityEngine;
using UnityEngine.Serialization;

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
    ///
    /// Déplacer : l'objet suit la manette ; en plus, le joystick le déplace dans toutes les directions
    ///   (avant / arrière / gauche / droite par rapport à l'endroit où l'on regarde ;
    ///   en maintenant le bouton de saisie (grip) : haut / bas = monter / descendre, jusqu'au sol du plateau).
    /// Rotation : joystick gauche / droite = tourner sur lui-même,
    ///   haut / bas = le basculer vers l'avant / l'arrière ;
    ///   le bouton « Align with floor » au-dessus de l'objet le pose parallèle au sol, sur la face la plus proche.
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
        [Tooltip("Vitesse (m/s) du déplacement au joystick, joystick à fond.")]
        [SerializeField, FormerlySerializedAs("pushPullSpeed")] float moveSpeed = 1f;
        [Tooltip("Empêche de descendre l'objet sous le sol du plateau. La hauteur du sol vient de la distance " +
                 "QR code → sol réglée dans Core/Resources/StageSettings.asset.")]
        [SerializeField] bool preventBelowStage = true;

        [Header("Scale")]
        [SerializeField] float scaleSpeed = 1f;      // facteur par seconde, joystick à fond
        [SerializeField] float minScale = 0.02f;     // en localScale.x
        [SerializeField] float maxScale = 5f;

        [Header("Rotation")]
        [SerializeField] float rotationSpeed = 90f;  // degrés par seconde
        [Tooltip("Joystick haut / bas en mode Rotation = basculer l'objet vers l'avant / l'arrière. " +
                 "Penser aussi à décocher « Keep Upright » sur Decor Constraints, sinon l'objet se redresse aussitôt.")]
        [SerializeField] bool allowTilt = true;

        public Mode CurrentMode { get; private set; } = Mode.None;

        Renderer[] _renderers;
        DecorInstance _decor;
        int _colorIndex;
        int _startFrame;

        // sauvegarde pour annuler
        Vector3 _savedPos, _savedScale;
        Quaternion _savedRot;

        // offset de l'objet dans le repère de la manette (pour Déplacer)
        Vector3 _grabLocalPos;
        // déplacement ajouté au joystick pendant Déplacer (repère du monde)
        Vector3 _stickOffset;
        Transform _head;

        void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _decor = GetComponent<DecorInstance>();
            if (Camera.main) _head = Camera.main.transform;

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
            {
                _grabLocalPos = controllerAnchor.InverseTransformPoint(transform.position);
                _stickOffset = Vector3.zero;
            }

            CurrentMode = mode;
            Active = this;
            _startFrame = Time.frameCount;

            // On cache le menu pendant la manipulation
            if (ObjectMenu.Instance) ObjectMenu.Instance.Hide();

            // En Rotation : bouton « Align with floor » au-dessus de l'objet
            if (mode == Mode.Rotate && allowTilt) LevelButton.Show(this, LevelWithFloor);
        }

        /// <summary>
        /// Pose l'objet parallèle au sol, sur la face la plus proche de sa position actuelle
        /// (voir DecorInstance.LevelWithStage). Appelé par le bouton « Align with floor ».
        /// </summary>
        public void LevelWithFloor()
        {
            if (!_decor) _decor = GetComponent<DecorInstance>();
            if (_decor)
            {
                // Autour du centre ; un objet au sol reste posé, un objet au cintre reste accroché.
                _decor.LevelWithStage();
            }
            else
            {
                Vector3 up = transform.parent ? transform.parent.up : Vector3.up;
                Vector3 closest = transform.up;
                foreach (var axis in new[] { transform.up, -transform.up, transform.right, -transform.right, transform.forward, -transform.forward })
                    if (Vector3.Dot(axis, up) > Vector3.Dot(closest, up)) closest = axis;
                transform.rotation = Quaternion.FromToRotation(closest, up) * transform.rotation;
            }
        }

        // ------------------------------------------------------------------

        void Update()
        {
            if (CurrentMode == Mode.None) return;

            // Ne pas valider dans la frame même où la manipulation a commencé
            if (Time.frameCount != _startFrame)
            {
                // La gâchette valide, sauf si le laser vise le bouton « Align with floor » (c'est alors un clic sur le bouton).
                bool onLevelButton = controllerAnchor &&
                                     LevelButton.IsPointerOver(new Ray(controllerAnchor.position, controllerAnchor.forward));
                if (!onLevelButton && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller)) { EndManipulation(true); return; }
                if (OVRInput.GetDown(OVRInput.Button.Two, controller)) { EndManipulation(false); return; } // B
            }

            Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller);
            if (Mathf.Abs(stick.x) < stickDeadZone) stick.x = 0f;
            if (Mathf.Abs(stick.y) < stickDeadZone) stick.y = 0f;
            float dt = Time.deltaTime;
            if (!_decor) _decor = GetComponent<DecorInstance>(); // ajouté après Awake par le spawner

            switch (CurrentMode)
            {
                case Mode.Move:
                {
                    // Le joystick déplace l'objet dans toutes les directions, par rapport à l'endroit où l'on regarde :
                    //   - haut / bas = avant / arrière, gauche / droite = gauche / droite ;
                    //   - en maintenant le grip : haut / bas = monter / descendre.
                    Vector3 forward = Vector3.forward, right = Vector3.right;
                    if (_head)
                    {
                        forward = Vector3.ProjectOnPlane(_head.forward, Vector3.up);
                        right = Vector3.ProjectOnPlane(_head.right, Vector3.up);
                        if (forward.sqrMagnitude < 1e-4f) forward = Vector3.ProjectOnPlane(_head.up, Vector3.up); // regard vertical
                        if (right.sqrMagnitude < 1e-4f) right = Vector3.Cross(Vector3.up, forward);
                        forward.Normalize();
                        right.Normalize();
                    }

                    bool vertical = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controller);
                    Vector3 direction = vertical ? Vector3.up * stick.y : forward * stick.y + right * stick.x;
                    _stickOffset += direction * moveSpeed * dt;

                    // L'objet suit aussi la manette, comme avant.
                    transform.position = controllerAnchor.TransformPoint(_grabLocalPos) + _stickOffset;

                    // Il ne passe pas sous le sol du plateau (option, cochée par défaut).
                    if (preventBelowStage && _decor) _decor.KeepAboveStage();
                    break;
                }

                case Mode.Scale:
                    // Joystick haut = agrandir, bas = rétrécir (uniforme).
                    // L'objet grandit autour de son point d'origine : un élément au sol reste posé
                    // sur le plateau, un élément au cintre reste accroché par le haut.
                    if (stick.y != 0f)
                    {
                        float current = transform.localScale.x;
                        float target = Mathf.Clamp(current * (1f + stick.y * scaleSpeed * dt), minScale, maxScale);
                        Vector3 newScale = transform.localScale * (target / current);

                        if (_decor)
                        {
                            // Cahier des charges : l'objet ne dépasse jamais 18 m sur sa plus grande dimension.
                            _decor.SetScaleAroundOrigin(_decor.ClampScaleToMaxSize(newScale));
                        }
                        else
                        {
                            transform.localScale = newScale;
                        }
                    }
                    break;

                case Mode.Rotate:
                {
                    // Joystick gauche / droite = tourner sur lui-même (autour de la verticale).
                    // Joystick haut / bas = basculer vers l'avant / l'arrière (autour de l'axe gauche-droite du regard).
                    float yaw = -stick.x * rotationSpeed * dt;
                    float tilt = allowTilt ? stick.y * rotationSpeed * dt : 0f;
                    Vector3 tiltAxis = _head ? Vector3.ProjectOnPlane(_head.right, Vector3.up) : Vector3.right;
                    if (tiltAxis.sqrMagnitude < 1e-4f) tiltAxis = Vector3.right;
                    tiltAxis.Normalize();

                    if (_decor)
                    {
                        // Autour du centre de l'objet ; un objet au sol reste posé, un objet au cintre reste accroché.
                        _decor.RotateAroundCenter(Vector3.up, yaw);
                        _decor.RotateAroundCenter(tiltAxis, tilt);
                    }
                    else
                    {
                        transform.Rotate(Vector3.up, yaw, Space.World);
                        transform.Rotate(tiltAxis, tilt, Space.World);
                    }
                    break;
                }
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
            LevelButton.Hide();

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
            if (Active == this) { Active = null; s_lastEndFrame = Time.frameCount; LevelButton.Hide(); }
            CurrentMode = Mode.None;
        }
    }
}
