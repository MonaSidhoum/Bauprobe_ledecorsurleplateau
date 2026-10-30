using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Rayon partant de la manette droite (à mettre sur un enfant de RightHandAnchor).
    /// - Affiche un laser (LineRenderer optionnel).
    /// - Gâchette droite sur un objet qui a un InteractableObject => ouvre le menu au-dessus.
    /// Les boutons du menu (Canvas) sont cliqués par l'OVRInputModule de l'EventSystem,
    /// exactement comme dans le sample QRCodeDetection.
    /// </summary>
    public class RightControllerRaySelector : MonoBehaviour
    {
        [SerializeField] OVRInput.Controller controller = OVRInput.Controller.RTouch;
        [SerializeField] float maxDistance = 10f;
        [Tooltip("Calques testés par le rayon (exclure par ex. les meshes de la pièce si besoin).")]
        [SerializeField] LayerMask raycastMask = ~0;

        [Header("Laser (optionnel)")]
        [SerializeField] LineRenderer laser;
        [SerializeField] Color idleColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] Color hoverColor = new Color(0.2f, 0.8f, 1f, 1f);

        void Update()
        {
            var ray = new Ray(transform.position, transform.forward);
            float endDistance = maxDistance;
            InteractableObject hovered = null;

            // 1) Le rayon est-il sur le menu ? (le menu n'a pas de collider : test plan + rectangle)
            bool overMenu = false;
            float menuDistance = float.MaxValue;
            if (ObjectMenu.Instance != null && ObjectMenu.Instance.RaycastMenu(ray, out menuDistance) && menuDistance <= maxDistance)
            {
                overMenu = true;
                endDistance = menuDistance;
            }

            // 2) Raycast physique vers les objets 3D (le cube)
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastMask, QueryTriggerInteraction.Ignore)
                && (!overMenu || hit.distance < menuDistance))
            {
                endDistance = hit.distance;
                hovered = hit.collider.GetComponentInParent<InteractableObject>();
            }

            // 3) Clic gâchette sur un objet interactif => menu
            //    (ignoré pendant un déplacement/scale/rotation, et dans la frame où il se termine,
            //     sinon la gâchette qui valide rouvrirait/recibleait le menu)
            bool busy = InteractableObject.IsAnyManipulating || InteractableObject.ManipulationEndedThisFrame;
            if (hovered != null && !busy && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
            {
                if (ObjectMenu.Instance != null)
                    ObjectMenu.Instance.Show(hovered);
            }

            // Pendant la Rotation, le laser sert à viser le bouton « Align with floor »
            bool overLevelButton = LevelButton.IsPointerOver(ray);

            UpdateLaser(ray, endDistance, hovered != null || overMenu || overLevelButton);
        }

        void UpdateLaser(Ray ray, float length, bool highlighted)
        {
            if (laser == null) return;

            // Pas de laser pendant une manipulation (on garde la vue dégagée),
            // sauf en Rotation, pour pouvoir viser le bouton « Align with floor ».
            var active = InteractableObject.Active;
            laser.enabled = active == null || active.CurrentMode == InteractableObject.Mode.Rotate;
            if (!laser.enabled) return;

            laser.useWorldSpace = true;
            laser.positionCount = 2;
            laser.SetPosition(0, ray.origin);
            laser.SetPosition(1, ray.GetPoint(length));

            Color c = highlighted ? hoverColor : idleColor;
            laser.startColor = c;
            laser.endColor = c;
        }
    }
}
