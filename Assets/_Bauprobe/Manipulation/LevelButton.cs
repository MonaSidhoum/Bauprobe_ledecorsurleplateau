using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Bouton « Remettre à plat » affiché au-dessus de l'objet pendant le mode Rotation.
    /// Un clic (laser + gâchette) remet l'objet parallèle au sol, sans changer son orientation gauche / droite.
    ///
    /// Le bouton est créé par le code la première fois qu'on en a besoin : il n'y a rien à ajouter
    /// dans la scène ni dans un prefab. Il est cliqué par l'OVRInputModule de l'EventSystem,
    /// comme les boutons du menu de l'objet.
    /// </summary>
    public class LevelButton : MonoBehaviour
    {
        const float CanvasScale = 0.001f;          // 1 unité du Canvas = 1 mm
        static readonly Vector2 Size = new Vector2(320f, 80f);
        const float GapAboveObject = 0.05f;        // en mètres

        static LevelButton s_instance;

        InteractableObject _target;
        Action _onClick;
        RectTransform _rect;
        Transform _camera;

        /// <summary>Affiche le bouton au-dessus de <paramref name="target"/> ; <paramref name="onClick"/> est appelé au clic.</summary>
        public static void Show(InteractableObject target, Action onClick)
        {
            if (s_instance == null)
            {
                s_instance = Create();
            }

            s_instance._target = target;
            s_instance._onClick = onClick;
            s_instance.gameObject.SetActive(true);
            s_instance.UpdatePose();
        }

        public static void Hide()
        {
            if (s_instance == null)
            {
                return;
            }

            s_instance._target = null;
            s_instance._onClick = null;
            s_instance.gameObject.SetActive(false);
        }

        /// <summary>
        /// Le rayon de la manette vise-t-il le bouton ? Sert à ce que la gâchette qui clique
        /// sur le bouton ne valide pas aussi la rotation.
        /// </summary>
        public static bool IsPointerOver(Ray ray)
        {
            if (s_instance == null || !s_instance.isActiveAndEnabled)
            {
                return false;
            }

            var t = s_instance.transform;
            var plane = new Plane(t.forward, t.position);
            if (!plane.Raycast(ray, out var distance))
            {
                return false;
            }

            var local = t.InverseTransformPoint(ray.GetPoint(distance));
            return s_instance._rect.rect.Contains(new Vector2(local.x, local.y));
        }

        void LateUpdate()
        {
            if (!_target)
            {
                Hide(); // objet supprimé entre-temps
                return;
            }

            UpdatePose();
        }

        void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
            }
        }

        // Au-dessus de l'objet (même penché), tourné vers la personne.
        void UpdatePose()
        {
            if (!_target)
            {
                return;
            }

            var b = _target.GetWorldBounds();
            var halfHeight = Size.y * CanvasScale * 0.5f;
            transform.position = b.center + Vector3.up * (b.extents.y + GapAboveObject + halfHeight);

            if (_camera == null && Camera.main != null)
            {
                _camera = Camera.main.transform;
            }

            if (_camera != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - _camera.position);
            }
        }

        void OnClicked()
        {
            _onClick?.Invoke();
        }

        // Construit le Canvas, le fond du bouton et son texte.
        static LevelButton Create()
        {
            var root = new GameObject("BoutonRemettreAPlat", typeof(RectTransform));
            root.SetActive(false);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main; // nécessaire pour l'OVRRaycaster
            root.AddComponent<OVRRaycaster>();

            var rect = (RectTransform)root.transform;
            rect.sizeDelta = Size;
            rect.localScale = Vector3.one * CanvasScale;

            // Fond cliquable
            var background = new GameObject("Fond", typeof(RectTransform));
            background.transform.SetParent(root.transform, false);
            Stretch((RectTransform)background.transform);
            var image = background.AddComponent<Image>();
            image.color = new Color(0.12f, 0.12f, 0.14f, 0.9f);
            var button = background.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.35f, 0.75f, 1f, 1f);
            colors.pressedColor = new Color(0.2f, 0.55f, 0.85f, 1f);
            button.colors = colors;

            // Texte
            var label = new GameObject("Texte", typeof(RectTransform));
            label.transform.SetParent(background.transform, false);
            Stretch((RectTransform)label.transform);
            var text = label.AddComponent<TextMeshProUGUI>();
            text.text = "Remettre à plat";
            text.fontSize = 36f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            var levelButton = root.AddComponent<LevelButton>();
            levelButton._rect = rect;
            button.onClick.AddListener(levelButton.OnClicked);
            return levelButton;
        }

        static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }
    }
}
