using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Base des petits panneaux créés par le code et affichés à la MAIN GAUCHE, à côté du menu général
    /// (INFO, pavé numérique du Scale, choix de la face en EXTRUDE).
    /// Fournit : la construction du Canvas (World Space), des textes et des boutons cliquables au laser
    /// (OVRRaycaster, comme les autres menus), le placement, et le test « le laser vise-t-il ce panneau ? ».
    /// Rien à ajouter dans la scène ni dans un prefab.
    /// </summary>
    public abstract class HandPanel : MonoBehaviour
    {
        protected static readonly Color PanelColor = new Color(0.12f, 0.12f, 0.14f, 0.92f);
        protected static readonly Color ButtonColor = new Color(0.25f, 0.25f, 0.28f, 1f);
        protected static readonly Color AccentColor = new Color(0.2f, 0.55f, 0.85f, 1f);
        protected const float GapMeters = 0.02f;

        static readonly List<HandPanel> s_visible = new List<HandPanel>();

        protected RectTransform Rect { get; private set; }
        Transform _camera;

        /// <summary>Le laser vise-t-il un des panneaux affichés ?</summary>
        public static bool IsPointerOverAnyPanel(Ray ray)
        {
            foreach (var panel in s_visible)
            {
                if (panel != null && panel.IsPointerOver(ray)) return true;
            }
            return false;
        }

        protected virtual void OnEnable()
        {
            if (!s_visible.Contains(this)) s_visible.Add(this);
        }

        protected virtual void OnDisable()
        {
            s_visible.Remove(this);
        }

        /// <summary>Le laser vise-t-il ce panneau (test plan + rectangle, le panneau n'a pas de collider) ?</summary>
        public bool IsPointerOver(Ray ray)
        {
            if (!isActiveAndEnabled || Rect == null) return false;

            var plane = new Plane(transform.forward, transform.position);
            if (!plane.Raycast(ray, out var distance)) return false;

            var local = transform.InverseTransformPoint(ray.GetPoint(distance));
            return Rect.rect.Contains(new Vector2(local.x, local.y));
        }

        // ------------------------------------------------------------------
        // Placement
        // ------------------------------------------------------------------

        /// <summary>
        /// À la main gauche, à côté du menu général (à la place du menu objet, caché pendant les manipulations).
        /// Sans menu général : au-dessus de l'objet, tourné vers la personne.
        /// </summary>
        protected void PlaceAtLeftHand(InteractableObject fallbackTarget)
        {
            if (GeneralMenuDock.TryPlace(Rect, GapMeters)) return;
            PlaceAboveObject(fallbackTarget);
        }

        /// <summary>À droite d'un autre panneau (ex. à droite du menu objet), dans le même plan que lui.</summary>
        protected void PlaceRightOf(RectTransform anchor)
        {
            if (anchor == null) return;

            Vector3 anchorScale = anchor.lossyScale;
            if (Mathf.Abs(anchorScale.x) < 1e-8f || Mathf.Abs(anchorScale.y) < 1e-8f) return;
            Vector3 scale = Rect.lossyScale;
            var size = new Vector2(Rect.rect.width * scale.x / anchorScale.x, Rect.rect.height * scale.y / anchorScale.y);
            float gap = GapMeters / Mathf.Abs(anchorScale.x);

            Vector2 center = MenuDocking.DockedCenter(anchor.rect, size, collapsed: false, gap);
            Vector2 pivotOffset = (Rect.pivot - new Vector2(0.5f, 0.5f)) * size;
            transform.position = anchor.TransformPoint(new Vector3(center.x + pivotOffset.x, center.y + pivotOffset.y, 0f));
            transform.rotation = anchor.rotation;
        }

        protected void PlaceAboveObject(InteractableObject target)
        {
            if (target == null) return;

            var b = target.GetWorldBounds();
            transform.position = b.center + Vector3.up * (b.extents.y + 0.05f + Rect.rect.height * Rect.lossyScale.y * 0.5f);

            if (_camera == null && Camera.main != null) _camera = Camera.main.transform;
            if (_camera != null) transform.rotation = Quaternion.LookRotation(transform.position - _camera.position);
        }

        // ------------------------------------------------------------------
        // Construction (unités du Canvas : 1 unité = canvasScale mètre)
        // ------------------------------------------------------------------

        /// <summary>Crée le panneau : Canvas World Space avec un fond. <paramref name="clickable"/> : boutons cliquables au laser.</summary>
        protected static T Create<T>(string objectName, Vector2 size, float canvasScale, bool clickable) where T : HandPanel
        {
            var root = new GameObject(objectName, typeof(RectTransform));
            root.SetActive(false);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main; // nécessaire pour l'OVRRaycaster
            if (clickable) root.AddComponent<OVRRaycaster>();

            var rect = (RectTransform)root.transform;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one * canvasScale;

            var background = root.AddComponent<Image>();
            background.color = PanelColor;
            background.raycastTarget = false;

            var panel = root.AddComponent<T>();
            panel.Rect = rect;
            return panel;
        }

        /// <summary>Texte placé dans un rectangle donné en unités du panneau, depuis son coin haut gauche.</summary>
        protected TextMeshProUGUI AddText(string text, float x, float y, float width, float height, float fontSize,
                                          TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
        {
            var go = new GameObject("Texte", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            PlaceTopLeft((RectTransform)go.transform, x, y, width, height);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        /// <summary>Bouton placé dans un rectangle donné en unités du panneau, depuis son coin haut gauche.</summary>
        protected Button AddButton(string label, float x, float y, float width, float height, UnityAction onClick,
                                   bool accent = false, float fontSize = 30f)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            PlaceTopLeft((RectTransform)go.transform, x, y, width, height);

            var image = go.AddComponent<Image>();
            image.color = accent ? AccentColor : ButtonColor;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.35f, 0.75f, 1f, 1f);
            colors.pressedColor = new Color(0.2f, 0.55f, 0.85f, 1f);
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(onClick);

            var textGo = new GameObject("Texte", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var textRect = (RectTransform)textGo.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return button;
        }

        static void PlaceTopLeft(RectTransform r, float x, float y, float width, float height)
        {
            r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(width, height);
        }
    }
}
