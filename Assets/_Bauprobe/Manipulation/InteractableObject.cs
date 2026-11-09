using System.Collections.Generic;
using Bauprobe.Decor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// À mettre sur le cube (ou sur la racine de n'importe quel objet avec un Collider).
    /// Contient les actions du menu : supprimer, couleur, déplacer, scale, rotation, étirer une face (EXTRUDE).
    ///
    /// Pendant Déplacer / Scale / Rotation :
    ///   - gâchette droite  = valider
    ///   - bouton B         = annuler (retour à l'état d'avant)
    ///   - joystick droit   = ajuster (voir chaque mode)
    ///
    /// Déplacer : l'objet est accroché à la manette, comme au bout du laser (tourner le poignet le déplace loin) ;
    ///   joystick haut / bas = l'éloigner / le rapprocher le long du laser, gauche / droite = le décaler sur le côté ;
    ///   grip maintenu + joystick haut / bas = monter / descendre. Plus l'objet est loin, plus il va vite.
    ///   Il ne descend pas sous le sol du plateau.
    /// Rotation : joystick gauche / droite = tourner sur lui-même, autour de la verticale seulement.
    /// Étirer (EXTRUDE) : viser une face au laser (elle s'éclaire en bleu) et appuyer sur la gâchette pour la choisir
    ///   (elle devient orange) ; puis joystick haut = pousser la face (allonger), bas = la rentrer (raccourcir),
    ///   grip maintenu = plus lent, pour être précis. La face opposée ne bouge pas. Gâchette = valider, B = annuler.
    /// </summary>
    public class InteractableObject : MonoBehaviour
    {
        public enum Mode { None, Move, Scale, Rotate, Extrude }

        // --- état global : une seule manipulation à la fois ---
        public static InteractableObject Active { get; private set; }
        public static bool IsAnyManipulating => Active != null;
        static int s_lastEndFrame = -1;
        public static bool ManipulationEndedThisFrame => s_lastEndFrame == Time.frameCount;

        [SerializeField] OVRInput.Controller controller = OVRInput.Controller.RTouch;
        [Tooltip("Ancre de la manette droite. Laissé vide = RightHandAnchor de l'OVRCameraRig, trouvé automatiquement.")]
        [SerializeField] Transform controllerAnchor;
        [SerializeField, Range(0f, 0.5f)] float stickDeadZone = 0.15f;

        [Tooltip("Remplace la saisie à la main (grab, Interaction SDK) par DecorGrabTransformer : l'objet garde ses " +
                 "modifications (taille, étirement…) quand on le prend à deux mains, et reste parallèle au sol.")]
        [SerializeField] bool useDecorGrab = true;

        [Header("Couleur")]
        [SerializeField] Color[] palette =
        {
            Color.white, new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.75f, 0.3f),
            new Color(0.2f, 0.45f, 0.95f), new Color(1f, 0.8f, 0.1f), new Color(0.7f, 0.3f, 0.9f)
        };

        [Header("Déplacer")]
        [Tooltip("Vitesse de base (m/s) du déplacement au joystick, joystick à fond, quand l'objet est tout près.")]
        [SerializeField, FormerlySerializedAs("pushPullSpeed")] float moveSpeed = 1f;
        [Tooltip("Vitesse en plus, proportionnelle à la distance : à 10 m de la manette, l'objet va " +
                 "1 + 10 × ce facteur m/s. Plus grand = plus d'amplitude au loin.")]
        [SerializeField, Min(0f)] float distanceSpeedFactor = 1f;
        [Tooltip("Seuil (en degrés) du joystick en Déplacer : poussé à moins de cet angle de l'avant / l'arrière, " +
                 "seul l'avant / arrière compte (pas de décalage sur le côté) ; à moins de cet angle de la gauche / " +
                 "droite, seul le côté compte. 0 = pas de seuil.")]
        [SerializeField, Range(0f, 45f)] float moveAxisSnapAngle = 25f;
        [Tooltip("Distance minimale (m) entre la manette et l'objet.")]
        [SerializeField, Min(0.05f)] float minMoveDistance = 0.3f;
        [Tooltip("Distance maximale (m) entre la manette et l'objet.")]
        [SerializeField, Min(1f)] float maxMoveDistance = 40f;
        [Tooltip("Empêche de descendre l'objet sous le sol du plateau. La hauteur du sol vient de la hauteur " +
                 "du QR code, saisie dans le casque (écran « QR code offset »).")]
        [SerializeField] bool preventBelowStage = true;

        [Header("Scale")]
        [SerializeField] float scaleSpeed = 1f;      // facteur par seconde, joystick à fond
        // Pas de taille minimale ni maximale : le Scale n'a plus de limite.

        [Header("Rotation")]
        [SerializeField] float rotationSpeed = 90f;  // degrés par seconde
        // Rotation seulement autour de la verticale (joystick gauche / droite) : plus de bascule avant / arrière.

        [Header("Étirer une face (EXTRUDE)")]
        [Tooltip("Vitesse (m/s) à laquelle la face avance ou recule, joystick à fond. " +
                 "En maintenant le grip, 5 fois plus lent pour être précis.")]
        [SerializeField] float extrudeSpeed = 0.5f;

        public Mode CurrentMode { get; private set; } = Mode.None;

        Renderer[] _renderers;
        DecorInstance _decor;
        Vector3 _originalScale = Vector3.one;
        int _colorIndex;
        int _startFrame;

        // sauvegarde pour annuler
        Vector3 _savedPos, _savedScale;
        Quaternion _savedRot;

        // face choisie (pour Étirer)
        DecorFace _extrudeFace;
        bool _faceSelected;

        // position de l'objet dans le repère de la manette (pour Déplacer) : il suit la manette comme
        // s'il était au bout du laser ; le joystick modifie cette position.
        Vector3 _grabLocalPos;
        Transform _head;

        void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _originalScale = transform.localScale; // taille d'origine si l'objet n'a pas de fiche de catalogue
            _decor = GetComponent<DecorInstance>();
            if (Camera.main) _head = Camera.main.transform;

            if (!controllerAnchor)
            {
                var rig = FindAnyObjectByType<OVRCameraRig>();
                if (rig) controllerAnchor = rig.rightHandAnchor;
            }

            // Saisie à la main (grab) : notre comportement à la place de celui du SDK (voir DecorGrabTransformer).
            // Doit être fait dans Awake, avant le Start du Grabbable.
            if (useDecorGrab)
            {
                DecorGrabTransformer.Install(GetComponent<Oculus.Interaction.Grabbable>());
            }
        }

        // ------------------------------------------------------------------
        // Liste des objets de la scène, et « suis-je à l'intérieur ? »
        // ------------------------------------------------------------------

        static readonly List<InteractableObject> s_all = new List<InteractableObject>();

        /// <summary>Tous les objets interactifs actifs de la scène.</summary>
        public static IReadOnlyList<InteractableObject> All => s_all;

        void OnEnable()
        {
            if (!s_all.Contains(this)) s_all.Add(this);
        }

        /// <summary>
        /// Le point est-il à l'intérieur de l'objet (de sa boîte englobante, qui suit sa rotation) ?
        /// Sert à sélectionner un objet quand on est dedans (ex. le module central), où le laser ne le touche pas.
        /// </summary>
        public bool Encloses(Vector3 worldPoint)
        {
            if (!_decor) _decor = GetComponent<DecorInstance>();
            if (_decor)
            {
                var b = _decor.LocalBounds;
                return b.size != Vector3.zero && b.Contains(transform.InverseTransformPoint(worldPoint));
            }
            return GetWorldBounds().Contains(worldPoint);
        }

        /// <summary>
        /// L'objet le plus petit qui contient ce point (null s'il n'y en a pas) : quand on est dans un objet
        /// lui-même posé dans un autre, c'est le plus proche de soi qui est choisi.
        /// </summary>
        public static InteractableObject FindEnclosing(Vector3 worldPoint)
        {
            InteractableObject best = null;
            float bestVolume = float.MaxValue;
            foreach (var o in s_all)
            {
                if (o == null || !o.Encloses(worldPoint)) continue;
                var size = o.GetWorldBounds().size;
                float volume = size.x * size.y * size.z;
                if (volume < bestVolume) { best = o; bestVolume = volume; }
            }
            return best;
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

            if (!_decor) _decor = GetComponent<DecorInstance>(); // ajouté après Awake par le spawner
            if (_decor)
            {
                _decor.SetColor(c); // retenue pour la sauvegarde
                return;
            }

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
            if ((mode == Mode.Move || mode == Mode.Extrude) && !controllerAnchor)
            {
                Debug.LogWarning($"{name}: pas d'ancre de manette, impossible de {(mode == Mode.Move ? "déplacer" : "viser une face")}.", this);
                return;
            }
            if (!_decor) _decor = GetComponent<DecorInstance>(); // ajouté après Awake par le spawner
            if (mode == Mode.Extrude && !_decor)
            {
                Debug.LogWarning($"{name}: seul un décor du catalogue peut être étiré.", this);
                return;
            }

            _savedPos = transform.position;
            _savedRot = transform.rotation;
            _savedScale = transform.localScale;

            if (mode == Mode.Move)
            {
                _grabLocalPos = controllerAnchor.InverseTransformPoint(transform.position);
            }
            _faceSelected = false;

            CurrentMode = mode;
            Active = this;
            _startFrame = Time.frameCount;

            // On cache le menu pendant la manipulation
            if (ObjectMenu.Instance) ObjectMenu.Instance.Hide();

            // En Scale : pavé numérique (taper un pourcentage) ; en EXTRUDE : boutons des faces. À la main gauche.
            if (mode == Mode.Scale) ScaleKeypad.Show(this);
            if (mode == Mode.Extrude) FacePicker.Show(this);
        }

        /// <summary>
        /// Donne à l'objet un pourcentage de sa taille D'ORIGINE (celle de son prefab dans le catalogue, avec les
        /// étirements faits par EXTRUDE) : 100 = taille d'origine en gardant la forme étirée, 150 = 1,5 fois, 50 = moitié.
        /// Le point d'origine ne bouge pas (un objet au sol reste posé). Appelé par le pavé numérique du Scale.
        /// </summary>
        public void ApplyScalePercentOfOriginal(float percent)
        {
            if (percent <= 0f || float.IsNaN(percent) || float.IsInfinity(percent)) return;
            if (!_decor) _decor = GetComponent<DecorInstance>();

            Vector3 original = _decor != null ? _decor.BaseScale : _originalScale;
            Vector3 newScale = original * (percent / 100f);

            if (_decor) _decor.SetScaleAroundOrigin(newScale);
            else transform.localScale = newScale;
        }

        /// <summary>
        /// Multiplie la taille de l'objet (ex. 1,5 = 150 %), en gardant ses proportions et son point d'origine
        /// (un objet au sol reste posé). Appelé par le pavé numérique du Scale.
        /// </summary>
        public void ApplyScaleFactor(float factor)
        {
            if (factor <= 0f || float.IsNaN(factor) || float.IsInfinity(factor)) return;
            if (!_decor) _decor = GetComponent<DecorInstance>();

            Vector3 newScale = transform.localScale * factor;
            if (_decor) _decor.SetScaleAroundOrigin(newScale);
            else transform.localScale = newScale;
        }

        /// <summary>
        /// Choisit la face à étirer en mode EXTRUDE (boutons TOP, FRONT… de la main gauche), sans la viser au laser.
        /// On peut en changer à tout moment.
        /// </summary>
        public void SelectExtrudeFace(DecorFace face)
        {
            if (CurrentMode != Mode.Extrude || !_decor) return;
            _extrudeFace = face;
            _faceSelected = true;
            FaceHighlight.Show(_decor, face, true);
        }

        // ------------------------------------------------------------------

        void Update()
        {
            if (CurrentMode == Mode.None) return;

            // Ne pas valider dans la frame même où la manipulation a commencé
            if (Time.frameCount != _startFrame)
            {
                // La gâchette valide, sauf :
                //  - si le laser vise un panneau de la main gauche (pavé du Scale, faces d'EXTRUDE, INFO) :
                //    c'est alors un clic sur ce panneau ;
                //  - en EXTRUDE tant qu'aucune face n'est choisie (la gâchette sert alors à choisir la face).
                bool onPanel = controllerAnchor &&
                                     ManipulationPanels.IsPointerOverAny(new Ray(controllerAnchor.position, controllerAnchor.forward));
                bool choosingFace = CurrentMode == Mode.Extrude && !_faceSelected;
                if (!onPanel && !choosingFace && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller)) { EndManipulation(true); return; }
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
                    // L'objet est accroché à la manette (comme au bout du laser) : tourner le poignet le déplace,
                    // d'autant plus loin qu'il est loin. Le joystick l'éloigne / le rapproche LE LONG DU LASER
                    // (la ligne manette → objet), le décale sur le côté, ou le monte / descend (grip maintenu).
                    Vector3 hand = controllerAnchor.position;
                    Vector3 target = controllerAnchor.TransformPoint(_grabLocalPos);
                    Vector3 toObject = target - hand;
                    float distance = toObject.magnitude;
                    Vector3 alongLaser = distance > 1e-4f ? toObject / distance : controllerAnchor.forward;

                    // Plus l'objet est loin, plus il va vite : assez d'amplitude au loin, assez de précision de près.
                    float speed = (moveSpeed + distanceSpeedFactor * distance) * dt;

                    // Seuil : un joystick poussé presque droit devant / derrière ne décale pas l'objet sur le côté
                    // (sinon il se met à tourner autour de soi), et inversement.
                    stick = MoveAxisLock(stick, moveAxisSnapAngle);

                    Vector3 delta;
                    if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controller))
                    {
                        delta = Vector3.up * (stick.y * speed);
                    }
                    else
                    {
                        Vector3 side = Vector3.Cross(Vector3.up, alongLaser); // vers la droite, à l'horizontale
                        if (side.sqrMagnitude < 1e-4f) side = _head ? _head.right : controllerAnchor.right;
                        side.Normalize();
                        delta = alongLaser * (stick.y * speed) + side * (stick.x * speed);
                    }

                    // Distance manette → objet entre le minimum et le maximum
                    Vector3 offset = target + delta - hand;
                    float newDistance = offset.magnitude;
                    if (newDistance < 1e-4f) offset = alongLaser * minMoveDistance;
                    else offset *= Mathf.Clamp(newDistance, minMoveDistance, maxMoveDistance) / newDistance;

                    transform.position = hand + offset;

                    // Il ne passe pas sous le sol du plateau (option, cochée par défaut).
                    if (preventBelowStage && _decor) _decor.KeepAboveStage();

                    // On garde la nouvelle position dans le repère de la manette : l'objet reste accroché au laser.
                    _grabLocalPos = controllerAnchor.InverseTransformPoint(transform.position);
                    break;
                }

                case Mode.Scale:
                    // Joystick haut = agrandir, bas = rétrécir (uniforme).
                    // L'objet grandit autour de son point d'origine : un élément au sol reste posé
                    // sur le plateau, un élément au cintre reste accroché par le haut.
                    if (stick.y != 0f)
                    {
                        // Agrandit tous les axes du même facteur : un objet étiré par une face garde sa forme.
                        // Pas de limite de taille ; le facteur reste juste positif (sinon l'objet se retournerait
                        // si une image dure très longtemps).
                        float factor = Mathf.Max(0.01f, 1f + stick.y * scaleSpeed * dt);
                        Vector3 newScale = transform.localScale * factor;

                        if (_decor)
                        {
                            _decor.SetScaleAroundOrigin(newScale);
                        }
                        else
                        {
                            transform.localScale = newScale;
                        }
                    }
                    break;

                case Mode.Rotate:
                {
                    // Joystick gauche / droite = tourner sur lui-même, autour de la verticale SEULEMENT
                    // (plus de bascule : l'objet ne peut plus partir en l'air).
                    float yaw = -stick.x * rotationSpeed * dt;

                    if (_decor)
                    {
                        // Autour du centre de l'objet ; un objet au sol reste posé, un objet au cintre reste accroché.
                        _decor.RotateAroundCenter(Vector3.up, yaw);
                    }
                    else
                    {
                        transform.Rotate(Vector3.up, yaw, Space.World);
                    }
                    break;
                }

                case Mode.Extrude:
                    UpdateExtrude(stick.y, dt);
                    break;
            }
        }

        /// <summary>
        /// Seuil du joystick : à moins de <paramref name="snapAngle"/> degrés de l'axe avant / arrière, on garde
        /// seulement l'avant / arrière ; à moins de snapAngle de l'axe gauche / droite, seulement le côté.
        /// Entre les deux (vraie diagonale), les deux sont gardés.
        /// </summary>
        public static Vector2 MoveAxisLock(Vector2 stick, float snapAngle)
        {
            if (snapAngle <= 0f || stick == Vector2.zero) return stick;

            // Angle du joystick par rapport à l'axe avant / arrière (0° = tout droit, 90° = sur le côté)
            float fromVertical = Mathf.Atan2(Mathf.Abs(stick.x), Mathf.Abs(stick.y)) * Mathf.Rad2Deg;
            if (fromVertical < snapAngle) return new Vector2(0f, stick.y);
            if (fromVertical > 90f - snapAngle) return new Vector2(stick.x, 0f);
            return stick;
        }

        // Étirer une face : 1) viser et choisir la face ; 2) la pousser / rentrer au joystick.
        void UpdateExtrude(float stickY, float dt)
        {
            if (!_decor) return;

            if (!_faceSelected)
            {
                // 1) La face visée par le laser s'éclaire ; gâchette = la choisir.
                //    (Sauf si le laser vise les boutons des faces de la main gauche : c'est eux qui choisissent.)
                var ray = new Ray(controllerAnchor.position, controllerAnchor.forward);
                if (!ManipulationPanels.IsPointerOverAny(ray)
                    && Physics.Raycast(ray, out var hit, 30f, ~0, QueryTriggerInteraction.Ignore)
                    && hit.collider.GetComponentInParent<InteractableObject>() == this
                    && _decor.TryGetFace(hit.point, out var face))
                {
                    if (Time.frameCount != _startFrame && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
                    {
                        _extrudeFace = face;
                        _faceSelected = true;
                    }
                    FaceHighlight.Show(_decor, face, _faceSelected);
                }
                else
                {
                    FaceHighlight.Hide();
                }
                return;
            }

            // 2) Joystick haut = pousser la face (allonger), bas = la rentrer (raccourcir). Grip = plus lent.
            if (stickY != 0f)
            {
                bool fine = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controller);
                _decor.ExtrudeFace(_extrudeFace, stickY * extrudeSpeed * (fine ? 0.2f : 1f) * dt);
            }
            FaceHighlight.Show(_decor, _extrudeFace, true);
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
            FaceHighlight.Hide();
            ScaleKeypad.Hide();
            FacePicker.Hide();
            _faceSelected = false;

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
            s_all.Remove(this);
            if (Active == this)
            {
                Active = null;
                s_lastEndFrame = Time.frameCount;
                FaceHighlight.Hide();
                ScaleKeypad.Hide();
                FacePicker.Hide();
            }
            CurrentMode = Mode.None;
        }
    }
}
