using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Menu de l'objet sélectionné (Canvas World Space). Un seul menu dans la scène, partagé par tous les objets.
    ///
    /// Il s'affiche quand on clique sur un objet, et se place à la MAIN GAUCHE, collé au menu général :
    ///   - menu général ouvert : à sa droite ;
    ///   - menu général replié (bouton − / +) : au-dessus du bouton « + ».
    /// Le menu général est trouvé tout seul sous la main gauche (objet « BauprobeMenu ») ; s'il n'y en a pas,
    /// le menu objet s'affiche au-dessus de l'objet, comme avant.
    /// IMPORTANT : laisser le GameObject ACTIF dans l'éditeur, il se cache tout seul au lancement.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class ObjectMenu : MonoBehaviour
    {
        public static ObjectMenu Instance { get; private set; }

        [Header("Boutons")]
        [SerializeField] Button closeButton;   // la croix en haut à droite
        [SerializeField] Button deleteButton;
        [SerializeField] Button colorButton;
        [SerializeField] Button moveButton;
        [SerializeField] Button scaleButton;
        [SerializeField] Button rotateButton;
        [Tooltip("Bouton EXTRUDE (étirer une face). Laissé vide = créé automatiquement en copiant le bouton Scale, " +
                 "juste en dessous de lui : pas besoin de modifier le prefab.")]
        [SerializeField] Button extrudeButton;

        [Header("Taille")]
        [Tooltip("Agrandit le menu (1 = taille du prefab). 2 = deux fois plus grand.")]
        [SerializeField, Min(0.1f)] float sizeMultiplier = 2f;

        [Header("Placement à côté du menu général (main gauche)")]
        [Tooltip("Coché : le menu objet se place à côté du menu général de la main gauche. " +
                 "Décoché : au-dessus de l'objet sélectionné.")]
        [SerializeField] bool dockToGeneralMenu = true;
        [Tooltip("Le menu général. Vide = trouvé automatiquement sous la main gauche (objet nommé « BauprobeMenu »).")]
        [SerializeField] RectTransform generalMenu;
        [Tooltip("Nom de l'objet du menu général, pour le trouver automatiquement sous la main gauche.")]
        [SerializeField] string generalMenuName = "BauprobeMenu";
        [Tooltip("Espace (en mètres) entre le menu général et le menu objet.")]
        [SerializeField] float gapToGeneralMenu = 0.02f;

        [Header("Placement au-dessus de l'objet (si pas de menu général)")]
        [Tooltip("Espace (en mètres) entre le haut de l'objet et le bas du menu.")]
        [SerializeField] float gapAboveObject = 0.05f;
        [SerializeField] bool faceCamera = true;

        public InteractableObject Target { get; private set; }

        Canvas _canvas;
        RectTransform _rect;
        Transform _camera;

        void Awake()
        {
            Instance = this;
            _canvas = GetComponent<Canvas>();
            _rect = (RectTransform)transform;

            // Menu plus grand (le prefab est réglé à 0,0005 : boutons de 10 cm de large)
            transform.localScale *= sizeMultiplier;

            // Menu général auprès duquel se placer (main gauche)
            if (!string.IsNullOrEmpty(generalMenuName)) GeneralMenuDock.MenuName = generalMenuName;
            if (generalMenu != null) GeneralMenuDock.SetMenu(generalMenu);

            if (Camera.main != null)
            {
                _camera = Camera.main.transform;
                _canvas.worldCamera = Camera.main; // nécessaire pour l'OVRRaycaster
            }

            closeButton.onClick.AddListener(Hide);
            deleteButton.onClick.AddListener(() => { if (Target) Target.Delete(); });
            colorButton.onClick.AddListener(() => { if (Target) Target.ChangeColor(); });
            moveButton.onClick.AddListener(() => { if (Target) Target.BeginManipulation(InteractableObject.Mode.Move); });
            scaleButton.onClick.AddListener(() => { if (Target) Target.BeginManipulation(InteractableObject.Mode.Scale); });
            rotateButton.onClick.AddListener(() => { if (Target) Target.BeginManipulation(InteractableObject.Mode.Rotate); });

            if (!extrudeButton) extrudeButton = CopyButton(scaleButton, "Btn_Extrude", "EXTRUDE");
            if (extrudeButton)
                extrudeButton.onClick.AddListener(() => { if (Target) Target.BeginManipulation(InteractableObject.Mode.Extrude); });

            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // Crée un nouveau bouton en copiant un bouton existant du menu (même style, même taille),
        // placé juste en dessous de lui. Le menu s'agrandit tout seul (Vertical Layout Group + Content Size Fitter).
        static Button CopyButton(Button model, string objectName, string label)
        {
            if (!model) return null;

            var copy = Instantiate(model, model.transform.parent);
            copy.name = objectName;
            copy.transform.SetSiblingIndex(model.transform.GetSiblingIndex() + 1);
            copy.onClick = new Button.ButtonClickedEvent(); // pas les actions du bouton copié

            var text = copy.GetComponentInChildren<TMP_Text>(true);
            if (text) text.text = label;
            return copy;
        }

        public void Show(InteractableObject target)
        {
            Target = target;
            gameObject.SetActive(true);
            UpdatePose();
        }

        public void Hide()
        {
            Target = null;
            gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (!Target) { Hide(); return; } // objet supprimé entre-temps
            UpdatePose();
        }

        void UpdatePose()
        {
            if (!Target) return;

            if (dockToGeneralMenu && GeneralMenuDock.TryPlace(_rect, gapToGeneralMenu)) return;

            // Pas de menu général : au-dessus de l'objet, tourné vers la personne
            Bounds b = Target.GetWorldBounds();
            float halfMenuHeight = _rect.rect.height * _rect.lossyScale.y * 0.5f;
            transform.position = b.center + Vector3.up * (b.extents.y + gapAboveObject + halfMenuHeight);

            if (faceCamera && _camera != null)
            {
                // Même logique que QRCodeFaceCamera du sample
                transform.rotation = Quaternion.LookRotation(transform.position - _camera.position);
            }
        }

        // ------------------------------------------------------------------
        // À côté du menu général (main gauche)
        // ------------------------------------------------------------------

        /// <summary>
        /// Le rayon touche-t-il le rectangle du menu ? Utilisé pour que la gâchette
        /// sur un bouton du menu ne sélectionne pas aussi un objet derrière.
        /// </summary>
        public bool RaycastMenu(Ray ray, out float distance)
        {
            distance = 0f;
            if (!isActiveAndEnabled) return false;

            var plane = new Plane(transform.forward, transform.position);
            if (!plane.Raycast(ray, out distance)) return false;

            Vector3 local = transform.InverseTransformPoint(ray.GetPoint(distance));
            return _rect.rect.Contains(new Vector2(local.x, local.y));
        }
    }
}
