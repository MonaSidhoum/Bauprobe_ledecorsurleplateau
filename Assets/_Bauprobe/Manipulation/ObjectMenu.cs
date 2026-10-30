using UnityEngine;
using UnityEngine.UI;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Menu contextuel (Canvas World Space) affiché au-dessus de l'objet sélectionné.
    /// Un seul menu dans la scène, partagé par tous les objets interactifs.
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

        [Header("Placement")]
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

            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
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

            Bounds b = Target.GetWorldBounds();
            float halfMenuHeight = _rect.rect.height * _rect.lossyScale.y * 0.5f;
            transform.position = b.center + Vector3.up * (b.extents.y + gapAboveObject + halfMenuHeight);

            if (faceCamera && _camera != null)
            {
                // Même logique que QRCodeFaceCamera du sample
                transform.rotation = Quaternion.LookRotation(transform.position - _camera.position);
            }
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
