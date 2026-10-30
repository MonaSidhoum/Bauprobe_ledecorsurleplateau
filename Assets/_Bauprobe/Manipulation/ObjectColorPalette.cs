using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Palette de couleurs du menu objet, dans un script séparé pour ne pas modifier
    /// ObjectMenu.cs ni InteractableObject.cs.
    ///
    /// À mettre sur la RACINE du prefab ObjectMenu, à côté du composant Object Menu.
    /// Au clic sur le bouton Color, une palette de pastilles rondes s'ouvre par-dessus les boutons ;
    /// un clic sur une pastille colore l'objet sélectionné et referme la palette.
    ///
    /// Champs à remplir dans l'Inspector : Color Button (Btn_Couleur) et Palette (le panneau Palette).
    /// </summary>
    public class ObjectColorPalette : MonoBehaviour
    {
        [Tooltip("Le bouton Color du menu (Btn_Couleur).")]
        [SerializeField] Button colorButton;

        [Tooltip("Le panneau Palette (enfant du menu). Sa taille et sa position sont réglées automatiquement.")]
        [SerializeField] RectTransform palette;

        [Tooltip("Nombre de pastilles par rangée.")]
        [SerializeField, Min(1)] int columns = 3;

        [Tooltip("Espace entre deux pastilles, en proportion de la taille d'une pastille.")]
        [SerializeField, Range(0f, 0.6f)] float swatchGap = 0.25f;

        /// <summary>15 teintes de décor de théâtre, en 5 rangées de 3 (une famille par rangée).</summary>
        static readonly Color[] Colors =
        {
            // Neutres clairs
            new Color32(0xF2, 0xEE, 0xE6, 255), // blanc cassé
            new Color32(0xE3, 0xD5, 0xB8, 255), // lin
            new Color32(0x8C, 0x8C, 0x8C, 255), // gris

            // Terres
            new Color32(0xD4, 0xA2, 0x4C, 255), // ocre
            new Color32(0xC2, 0x68, 0x3F, 255), // terracotta
            new Color32(0x7A, 0x52, 0x36, 255), // bois

            // Rouges
            new Color32(0xD9, 0xA3, 0xA8, 255), // rose poudré
            new Color32(0xB2, 0x3A, 0x3A, 255), // rouge théâtre
            new Color32(0x6E, 0x1F, 0x2E, 255), // bordeaux

            // Bleus
            new Color32(0x8F, 0xA9, 0xC2, 255), // bleu gris
            new Color32(0x1F, 0x5C, 0x66, 255), // bleu canard
            new Color32(0x1F, 0x2A, 0x44, 255), // bleu nuit

            // Verts et sombre
            new Color32(0x9A, 0xAE, 0x8C, 255), // vert sauge
            new Color32(0x2F, 0x4A, 0x3A, 255), // vert sapin
            new Color32(0x2E, 0x2E, 0x30, 255), // anthracite
        };

        // URP / HDRP utilisent _BaseColor, le pipeline intégré (Standard) utilise _Color.
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        // Couleur choisie pour chaque objet (pour mettre en avant la pastille actuelle).
        readonly Dictionary<InteractableObject, int> _chosen = new Dictionary<InteractableObject, int>();
        readonly List<GameObject> _swatches = new List<GameObject>();
        InteractableObject _openFor;

        void Start()
        {
            // ObjectMenu branche déjà le bouton Color sur "couleur suivante" :
            // on retire ce branchement (fait par code) et on met le nôtre à la place.
            if (colorButton != null)
            {
                colorButton.onClick.RemoveAllListeners();
                colorButton.onClick.AddListener(TogglePalette);
            }
            else
            {
                Debug.LogWarning("[ObjectColorPalette] Assigne 'Color Button' dans l'Inspector.", this);
            }

            SetupPaletteLayout();
            ClosePalette();
        }

        void OnEnable()
        {
            // Le menu vient de s'ouvrir : la palette doit être fermée.
            ClosePalette();
        }

        void Update()
        {
            // Si on clique sur un autre objet pendant que la palette est ouverte, on la referme.
            if (_openFor != null && (ObjectMenu.Instance == null || ObjectMenu.Instance.Target != _openFor))
                ClosePalette();
        }

        // ------------------------------------------------------------------ ouverture / fermeture

        void TogglePalette()
        {
            var target = ObjectMenu.Instance != null ? ObjectMenu.Instance.Target : null;
            if (target == null || palette == null) return;

            if (palette.gameObject.activeSelf) { ClosePalette(); return; }

            BuildSwatches(target);
            palette.gameObject.SetActive(true);
            _openFor = target;
        }

        void ClosePalette()
        {
            if (palette != null) palette.gameObject.SetActive(false);
            _openFor = null;
        }

        // ------------------------------------------------------------------ couleur

        void ApplyColor(InteractableObject target, int index)
        {
            if (target == null) return;
            _chosen[target] = index;
            Color c = Colors[index];

            foreach (var r in target.GetComponentsInChildren<Renderer>())
            {
                // .materials crée une copie propre à cet objet : les autres objets ne changent pas.
                foreach (var mat in r.materials)
                {
                    if (mat.HasProperty(BaseColorId)) mat.SetColor(BaseColorId, c);
                    else if (mat.HasProperty(ColorId)) mat.SetColor(ColorId, c);
                }
            }
        }

        // ------------------------------------------------------------------ pastilles

        void BuildSwatches(InteractableObject target)
        {
            foreach (var s in _swatches) if (s) { s.SetActive(false); Destroy(s); }
            _swatches.Clear();

            FitGrid(Colors.Length);
            _chosen.TryGetValue(target, out int current);
            bool hasChosen = _chosen.ContainsKey(target);

            for (int i = 0; i < Colors.Length; i++)
            {
                int index = i; // copie locale, sinon tous les boutons utiliseraient le dernier i

                var go = new GameObject("Pastille " + i, typeof(RectTransform), typeof(Image), typeof(Button));
                go.layer = palette.gameObject.layer;
                go.transform.SetParent(palette, false);

                // Pastille ronde, sans bord
                var image = go.GetComponent<Image>();
                image.sprite = CircleSprite();
                image.preserveAspect = true;
                image.color = Colors[i];

                // La couleur actuelle de l'objet est un peu plus grosse que les autres
                if (hasChosen && i == current) go.transform.localScale = Vector3.one * 1.2f;

                var button = go.GetComponent<Button>();
                button.targetGraphic = image;
                var block = button.colors;
                block.highlightedColor = new Color(0.85f, 0.85f, 0.85f, 1f); // légèrement assombrie au survol
                block.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                button.colors = block;
                button.onClick.AddListener(() =>
                {
                    ApplyColor(_openFor, index);
                    ClosePalette();
                });

                _swatches.Add(go);
            }
        }

        // ------------------------------------------------------------------ mise en page

        /// <summary>
        /// Place la palette par-dessus la zone des boutons (sans cacher l'en-tête où est la croix)
        /// et l'exclut de la mise en page du menu, pour qu'elle ne le fasse pas grandir.
        /// </summary>
        void SetupPaletteLayout()
        {
            if (palette == null) return;

            var element = palette.GetComponent<LayoutElement>();
            if (element == null) element = palette.gameObject.AddComponent<LayoutElement>();
            element.ignoreLayout = true;

            var layout = GetComponent<VerticalLayoutGroup>();
            RectOffset pad = layout != null ? layout.padding : new RectOffset(12, 12, 52, 12);

            palette.anchorMin = Vector2.zero;
            palette.anchorMax = Vector2.one;
            palette.pivot = new Vector2(0.5f, 0.5f);
            palette.offsetMin = new Vector2(pad.left, pad.bottom);
            palette.offsetMax = new Vector2(-pad.right, -pad.top);
            palette.localScale = Vector3.one;
            palette.localRotation = Quaternion.identity;
            palette.SetAsLastSibling();
        }

        /// <summary>Règle la grille pour que toutes les pastilles tiennent dans le panneau, carrées et centrées.</summary>
        void FitGrid(int count)
        {
            var grid = palette.GetComponent<GridLayoutGroup>();
            if (grid == null) grid = palette.gameObject.AddComponent<GridLayoutGroup>();

            int cols = Mathf.Clamp(columns, 1, count);
            int rows = Mathf.CeilToInt(count / (float)cols);

            Rect area = palette.rect;
            float availW = area.width - grid.padding.left - grid.padding.right;
            float availH = area.height - grid.padding.top - grid.padding.bottom;

            float sizeW = availW / (cols + swatchGap * (cols - 1));
            float sizeH = availH / (rows + swatchGap * (rows - 1));
            float size = Mathf.Max(1f, Mathf.Min(sizeW, sizeH) * 0.9f); // 10 % de marge autour

            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = cols;
            grid.cellSize = new Vector2(size, size);
            grid.spacing = new Vector2(size * swatchGap, size * swatchGap);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        }

        // Disque blanc lissé, créé une seule fois et teinté par chaque pastille.
        static Sprite s_circle;

        static Sprite CircleSprite()
        {
            if (s_circle != null) return s_circle;

            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear
            };

            var pixels = new Color32[size * size];
            float radius = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - radius;
                    float dy = y + 0.5f - radius;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01((radius - dist) / 1.5f); // bord adouci
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(true);

            s_circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return s_circle;
        }
    }
}
