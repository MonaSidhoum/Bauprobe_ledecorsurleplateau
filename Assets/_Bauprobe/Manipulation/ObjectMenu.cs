using System.Collections.Generic;
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

        // Menu général : hauteur quand il est ouvert (la plus grande hauteur visible observée)
        float _generalMenuOpenHeight;
        bool _searchedGeneralMenu;
        readonly List<Graphic> _graphics = new List<Graphic>();
        readonly Vector3[] _corners = new Vector3[4];

        void Awake()
        {
            Instance = this;
            _canvas = GetComponent<Canvas>();
            _rect = (RectTransform)transform;

            // Menu plus grand (le prefab est réglé à 0,0005 : boutons de 10 cm de large)
            transform.localScale *= sizeMultiplier;

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

            if (dockToGeneralMenu && TryDockToGeneralMenu()) return;

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

        /// <summary>Place le menu à côté du menu général. Renvoie false si le menu général est introuvable ou caché.</summary>
        bool TryDockToGeneralMenu()
        {
            var dock = FindGeneralMenu();
            if (dock == null || !dock.gameObject.activeInHierarchy) return false;
            if (!TryGetVisibleRect(dock, out var visible)) return false;

            // Replié ou ouvert ? (replié = il ne reste que le petit bouton « + »)
            _generalMenuOpenHeight = Mathf.Max(_generalMenuOpenHeight, visible.height, dock.rect.height);
            bool collapsed = MenuDocking.IsCollapsed(visible.height, _generalMenuOpenHeight);

            // Taille du menu objet et espace, convertis dans les unités du menu général
            Vector3 dockScale = dock.lossyScale;
            if (Mathf.Abs(dockScale.x) < 1e-8f || Mathf.Abs(dockScale.y) < 1e-8f) return false;
            Vector3 menuScale = _rect.lossyScale;
            var menuSize = new Vector2(_rect.rect.width * menuScale.x / dockScale.x,
                                       _rect.rect.height * menuScale.y / dockScale.y);
            float gap = gapToGeneralMenu / Mathf.Abs(dockScale.y);

            Vector2 center = MenuDocking.DockedCenter(visible, menuSize, collapsed, gap);

            // Le centre du menu objet est son pivot seulement si le pivot est au milieu : on corrige sinon.
            Vector2 pivotOffset = (_rect.pivot - new Vector2(0.5f, 0.5f)) * menuSize;
            transform.position = dock.TransformPoint(new Vector3(center.x + pivotOffset.x, center.y + pivotOffset.y, 0f));
            transform.rotation = dock.rotation; // dans le même plan que le menu général
            return true;
        }

        RectTransform FindGeneralMenu()
        {
            if (generalMenu != null || _searchedGeneralMenu) return generalMenu;
            _searchedGeneralMenu = true;

            var rig = FindAnyObjectByType<OVRCameraRig>();
            if (rig == null || rig.leftHandAnchor == null) return null;

            foreach (var rt in rig.leftHandAnchor.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt.name == generalMenuName)
                {
                    generalMenu = rt;
                    return generalMenu;
                }
            }

            Debug.LogWarning($"[ObjectMenu] Menu général « {generalMenuName} » introuvable sous la main gauche : " +
                             "le menu objet s'affichera au-dessus de l'objet.", this);
            return null;
        }

        /// <summary>
        /// Rectangle de la partie VISIBLE du menu général (dans son repère local) : on réunit les éléments
        /// affichés (images, textes) dont la transparence n'est pas nulle. Quand le menu est replié,
        /// seul le bouton « + » est visible.
        /// </summary>
        bool TryGetVisibleRect(RectTransform dock, out Rect visible)
        {
            visible = default;
            bool any = false;
            float xMin = 0f, xMax = 0f, yMin = 0f, yMax = 0f;

            dock.GetComponentsInChildren(false, _graphics);
            foreach (var g in _graphics)
            {
                if (!g.isActiveAndEnabled || g.color.a < 0.01f || g.canvasRenderer.GetInheritedAlpha() < 0.01f) continue;

                g.rectTransform.GetWorldCorners(_corners);
                for (int i = 0; i < 4; i++)
                {
                    Vector3 p = dock.InverseTransformPoint(_corners[i]);
                    if (!any) { xMin = xMax = p.x; yMin = yMax = p.y; any = true; }
                    else
                    {
                        xMin = Mathf.Min(xMin, p.x); xMax = Mathf.Max(xMax, p.x);
                        yMin = Mathf.Min(yMin, p.y); yMax = Mathf.Max(yMax, p.y);
                    }
                }
            }

            if (!any) return false;
            visible = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
            return true;
        }

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
