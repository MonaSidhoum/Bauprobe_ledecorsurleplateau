using Bauprobe.Decor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Rectangle semi-transparent posé sur une face d'un objet, avec sa longueur affichée.
    /// Utilisé par le mode EXTRUDE (étirer une face) : bleu = face visée, orange = face choisie.
    ///
    /// Créé par le code la première fois qu'on en a besoin : rien à ajouter dans la scène ni dans un prefab.
    /// Il ne bloque pas le laser (pas de raycaster).
    /// </summary>
    public class FaceHighlight : MonoBehaviour
    {
        const float CanvasScale = 0.001f;            // 1 unité du Canvas = 1 mm
        const float OffsetFromFace = 0.003f;         // 3 mm devant la face, pour ne pas clignoter avec elle
        static readonly Color HoverColor = new Color(0.35f, 0.75f, 1f, 0.35f);
        static readonly Color SelectedColor = new Color(1f, 0.6f, 0.15f, 0.45f);

        static FaceHighlight s_instance;

        DecorInstance _target;
        DecorFace _face;
        bool _selected;
        RectTransform _rect;
        Image _image;
        TextMeshProUGUI _label;
        Transform _camera;

        /// <summary>Affiche le rectangle sur la face <paramref name="face"/> de <paramref name="target"/>.</summary>
        public static void Show(DecorInstance target, DecorFace face, bool selected)
        {
            if (target == null)
            {
                Hide();
                return;
            }

            if (s_instance == null)
            {
                s_instance = Create();
            }

            s_instance._target = target;
            s_instance._face = face;
            s_instance._selected = selected;
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
            s_instance.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (_target == null)
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

        void UpdatePose()
        {
            _target.GetFaceGeometry(_face, out var center, out var normal, out var sideU, out var sideV);

            // Choisir le côté qui sert de « haut » pour que le texte soit lisible :
            // sur une face verticale, le côté vertical ; sur une face horizontale, celui qui part devant soi.
            if (_camera == null && Camera.main != null)
            {
                _camera = Camera.main.transform;
            }
            var reference = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.7f && _camera != null)
            {
                reference = Vector3.ProjectOnPlane(_camera.forward, Vector3.up);
            }

            var up = sideV;
            var across = sideU;
            if (Mathf.Abs(Vector3.Dot(sideU.normalized, reference)) > Mathf.Abs(Vector3.Dot(sideV.normalized, reference)))
            {
                up = sideU;
                across = sideV;
            }
            if (Vector3.Dot(up, reference) < 0f)
            {
                up = -up;
            }

            transform.position = center + normal * OffsetFromFace;
            // L'avant du Canvas pointe vers l'intérieur de l'objet : on voit donc son recto depuis l'extérieur.
            transform.rotation = Quaternion.LookRotation(-normal, up.sqrMagnitude > 1e-8f ? up.normalized : Vector3.up);
            _rect.sizeDelta = new Vector2(across.magnitude, up.magnitude) / CanvasScale;

            _image.color = _selected ? SelectedColor : HoverColor;
            _label.text = $"{_target.GetLength(_face.Axis):0.00} m";
        }

        // Construit le Canvas, le rectangle et le texte.
        static FaceHighlight Create()
        {
            var root = new GameObject("FaceHighlight", typeof(RectTransform));
            root.SetActive(false);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            root.transform.localScale = Vector3.one * CanvasScale;

            var image = root.AddComponent<Image>();
            image.raycastTarget = false;

            // Texte : taille fixe (6 cm de haut), centré sur la face, sans retour à la ligne.
            var label = new GameObject("Longueur", typeof(RectTransform));
            label.transform.SetParent(root.transform, false);
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(600f, 100f);
            var text = label.AddComponent<TextMeshProUGUI>();
            text.fontSize = 60f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            var highlight = root.AddComponent<FaceHighlight>();
            highlight._rect = (RectTransform)root.transform;
            highlight._image = image;
            highlight._label = text;
            return highlight;
        }
    }
}
