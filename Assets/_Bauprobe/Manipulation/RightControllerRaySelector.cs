using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Rayon partant de la manette droite (à mettre sur un enfant de RightHandAnchor).
    /// - Affiche un laser (LineRenderer optionnel).
    /// - Gâchette droite sur un objet qui a un InteractableObject => ouvre le menu au-dessus.
    /// Les boutons du menu (Canvas) sont cliqués par l'OVRInputModule de l'EventSystem,
    /// exactement comme dans le sample QRCodeDetection.
    ///
    /// Pendant Déplacer, notre laser relie la manette à l'objet ; le laser de Meta (OVRRayHelper de la manette
    /// droite, qui part toujours tout droit) est caché pour ne pas avoir deux lasers, puis réaffiché ensuite.
    /// </summary>
    [DefaultExecutionOrder(1000)] // LateUpdate après les scripts de Meta : le laser Meta reste bien caché
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

        [Header("Laser Meta")]
        [Tooltip("Cache le laser de Meta (OVRRayHelper de la manette droite) pendant Déplacer, " +
                 "pour qu'on ne voie qu'un seul laser : le nôtre, qui va de la manette à l'objet.")]
        [SerializeField] bool hideMetaRayWhileMoving = true;

        Renderer[] _metaRayRenderers;
        float _nextMetaRaySearch;
        readonly List<Renderer> _hiddenMetaRenderers = new List<Renderer>();

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

        void LateUpdate()
        {
            var active = InteractableObject.Active;
            bool moving = hideMetaRayWhileMoving && active != null && active.CurrentMode == InteractableObject.Mode.Move;
            if (moving) HideMetaRay();
            else RestoreMetaRay();
        }

        // ------------------------------------------------------------------
        // Laser de Meta (OVRRayHelper) : caché pendant Déplacer
        // ------------------------------------------------------------------

        void HideMetaRay()
        {
            var renderers = FindMetaRayRenderers();
            if (renderers == null) return;

            // Re-caché à chaque image (au cas où un script Meta le réactive)
            foreach (var r in renderers)
            {
                if (r != null && r.enabled)
                {
                    r.enabled = false;
                    if (!_hiddenMetaRenderers.Contains(r)) _hiddenMetaRenderers.Add(r);
                }
            }
        }

        void RestoreMetaRay()
        {
            if (_hiddenMetaRenderers.Count == 0) return;
            foreach (var r in _hiddenMetaRenderers)
            {
                if (r != null) r.enabled = true;
            }
            _hiddenMetaRenderers.Clear();
        }

        /// <summary>
        /// Les Renderers du laser Meta de cette manette : le champ « RayHelper » de l'OVRControllerHelper
        /// dont le champ « m_controller » est notre manette (ici : [BuildingBlock] Controller Tracking Right).
        /// Lus par leur nom (réflexion) pour ne pas dépendre de la version du SDK Meta.
        /// </summary>
        Renderer[] FindMetaRayRenderers()
        {
            if (_metaRayRenderers != null || Time.unscaledTime < _nextMetaRaySearch) return _metaRayRenderers;
            _nextMetaRaySearch = Time.unscaledTime + 2f; // si introuvable, on réessaie dans 2 s

            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (mb == null || mb.GetType().Name != "OVRControllerHelper") continue;

                var type = mb.GetType();
                var controllerField = type.GetField("m_controller", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var rayField = type.GetField("RayHelper", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (controllerField == null || rayField == null) continue;
                if (!(controllerField.GetValue(mb) is OVRInput.Controller c) || c != controller) continue;

                if (rayField.GetValue(mb) is Component ray && ray != null)
                {
                    _metaRayRenderers = ray.GetComponentsInChildren<Renderer>(true);
                    return _metaRayRenderers;
                }
            }

            return null;
        }

        void UpdateLaser(Ray ray, float length, bool highlighted)
        {
            if (laser == null) return;

            var active = InteractableObject.Active;

            // Pendant Déplacer : le laser relie la manette au centre de l'objet déplacé (il le « tient »).
            if (active != null && active.CurrentMode == InteractableObject.Mode.Move)
            {
                laser.enabled = true;
                laser.useWorldSpace = true;
                laser.positionCount = 2;
                laser.SetPosition(0, ray.origin);
                laser.SetPosition(1, active.GetWorldBounds().center);
                laser.startColor = hoverColor;
                laser.endColor = hoverColor;
                return;
            }

            // Pas de laser pendant Scale (on garde la vue dégagée),
            // mais oui en Rotation (pour viser le bouton « Align with floor ») et en EXTRUDE (pour viser une face).
            laser.enabled = active == null
                            || active.CurrentMode == InteractableObject.Mode.Rotate
                            || active.CurrentMode == InteractableObject.Mode.Extrude;
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
