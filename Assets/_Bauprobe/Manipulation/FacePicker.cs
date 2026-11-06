using Bauprobe.Decor;
using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Choix de la face à étirer, à la main gauche, pendant le mode EXTRUDE (en anglais) :
    /// TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK. Utile quand on ne peut pas viser la face au laser
    /// (face du haut d'un grand décor, ou quand on est à l'intérieur de l'objet).
    ///
    /// Les côtés sont vus depuis la personne qui porte le casque : FRONT = la face tournée vers elle,
    /// RIGHT = la face à sa droite, TOP = la face tournée vers le haut. On peut changer de face à tout moment.
    /// </summary>
    public class FacePicker : HandPanel
    {
        /// <summary>Les côtés proposés, vus depuis la personne.</summary>
        public enum Side { Top, Bottom, Left, Right, Front, Back }

        static FacePicker s_instance;

        InteractableObject _target;
        Transform _head;

        public static void Show(InteractableObject target)
        {
            if (target == null) return;
            if (s_instance == null) s_instance = Build();

            s_instance._target = target;
            s_instance.gameObject.SetActive(true);
            s_instance.PlaceAtLeftHand(target);
        }

        public static void Hide()
        {
            if (s_instance == null) return;
            s_instance._target = null;
            s_instance.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (_target == null) { Hide(); return; }
            PlaceAtLeftHand(_target);
        }

        void OnDestroy()
        {
            if (s_instance == this) s_instance = null;
        }

        void Pick(Side side)
        {
            if (_target == null) return;
            var decor = _target.GetComponent<DecorInstance>();
            if (decor == null) return;

            if (_head == null && Camera.main != null) _head = Camera.main.transform;
            Vector3 viewer = _head != null ? _head.position : decor.transform.position - Vector3.forward;
            _target.SelectExtrudeFace(decor.FaceTowards(DirectionOf(side, decor, viewer)));
        }

        /// <summary>
        /// Direction (monde) d'un côté vu depuis <paramref name="viewer"/> :
        /// TOP / BOTTOM = haut / bas du plateau ; FRONT = vers la personne ; RIGHT = à sa droite.
        /// </summary>
        public static Vector3 DirectionOf(Side side, DecorInstance decor, Vector3 viewer)
        {
            Vector3 up = decor.transform.parent != null ? decor.transform.parent.up : Vector3.up;
            Vector3 center = decor.transform.TransformPoint(decor.LocalBounds.center);

            // Vers la personne, à l'horizontale
            Vector3 toViewer = Vector3.ProjectOnPlane(viewer - center, up);
            if (toViewer.sqrMagnitude < 1e-6f) toViewer = Vector3.ProjectOnPlane(-decor.transform.forward, up);
            if (toViewer.sqrMagnitude < 1e-6f) toViewer = Vector3.ProjectOnPlane(Vector3.back, up);
            toViewer.Normalize();
            Vector3 viewerRight = Vector3.Cross(up, -toViewer); // la droite de la personne qui regarde l'objet

            switch (side)
            {
                case Side.Top: return up;
                case Side.Bottom: return -up;
                case Side.Front: return toViewer;
                case Side.Back: return -toViewer;
                case Side.Right: return viewerRight;
                default: return -viewerRight; // Left
            }
        }

        static FacePicker Build()
        {
            // 36 x 26 cm (1 unité = 1 mm)
            var panel = Create<FacePicker>("FacePicker", new Vector2(360f, 260f), 0.001f, clickable: true);
            panel.AddText("<b>EXTRUDE</b>  select a face\n(or aim at it with the laser)", 20f, 12f, 320f, 64f, 20f);

            const float width = 100f, height = 70f, gap = 10f, left = 25f, top = 88f;
            panel.AddButton("LEFT", left, top, width, height, () => panel.Pick(Side.Left), fontSize: 24f);
            panel.AddButton("TOP", left + width + gap, top, width, height, () => panel.Pick(Side.Top), fontSize: 24f);
            panel.AddButton("RIGHT", left + 2 * (width + gap), top, width, height, () => panel.Pick(Side.Right), fontSize: 24f);
            panel.AddButton("FRONT", left, top + height + gap, width, height, () => panel.Pick(Side.Front), fontSize: 24f);
            panel.AddButton("BOTTOM", left + width + gap, top + height + gap, width, height, () => panel.Pick(Side.Bottom), fontSize: 22f);
            panel.AddButton("BACK", left + 2 * (width + gap), top + height + gap, width, height, () => panel.Pick(Side.Back), fontSize: 24f);
            return panel;
        }
    }
}
