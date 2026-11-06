using System.Globalization;
using System.Text;
using Bauprobe.Core;
using Bauprobe.Decor;
using TMPro;
using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Panneau « OBJECT INFO » (bouton INFO du menu objet), à droite du menu objet, en anglais :
    /// position, rotation, dimensions, hauteur et volume de l'objet sélectionné, mis à jour en direct.
    ///
    /// Mêmes valeurs et même repère que le tableau CSV du bureau d'études (voir LayoutCsv / StagePlan) :
    /// position du point d'origine (bas au centre pour le sol, haut au centre pour le cintre) depuis le point zéro,
    /// X + à droite, Y + vers le lointain, Z = hauteur au-dessus du sol du plateau.
    /// Le volume est celui de la boîte englobante (largeur × profondeur × hauteur).
    /// </summary>
    public class ObjectInfoPanel : HandPanel
    {
        static ObjectInfoPanel s_instance;

        InteractableObject _target;
        TextMeshProUGUI _text;
        readonly StringBuilder _sb = new StringBuilder();

        /// <summary>Affiche le panneau pour cet objet, ou le cache s'il est déjà affiché pour lui.</summary>
        public static void Toggle(InteractableObject target)
        {
            if (s_instance != null && s_instance.isActiveAndEnabled && s_instance._target == target)
            {
                Hide();
                return;
            }
            Show(target);
        }

        public static void Show(InteractableObject target)
        {
            if (target == null) return;
            if (s_instance == null) s_instance = Build();

            s_instance._target = target;
            s_instance.gameObject.SetActive(true);
            s_instance.Refresh();
        }

        public static void Hide()
        {
            if (s_instance == null) return;
            s_instance._target = null;
            s_instance.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            // Le panneau va avec le menu objet : il disparaît quand le menu se ferme ou change d'objet.
            var menu = ObjectMenu.Instance;
            if (_target == null || menu == null || !menu.isActiveAndEnabled || menu.Target != _target)
            {
                Hide();
                return;
            }
            Refresh();
        }

        void OnDestroy()
        {
            if (s_instance == this) s_instance = null;
        }

        void Refresh()
        {
            var menu = ObjectMenu.Instance;
            if (menu != null && menu.isActiveAndEnabled) PlaceRightOf((RectTransform)menu.transform);
            else PlaceAtLeftHand(_target);

            _text.text = Describe(_target, _sb);
        }

        /// <summary>Texte du panneau (en anglais).</summary>
        static string Describe(InteractableObject target, StringBuilder sb)
        {
            sb.Clear();
            var decor = target.GetComponent<DecorInstance>();
            var t = target.transform;

            string name = decor != null && decor.Definition != null ? decor.Definition.DisplayName : target.name;
            bool flown = decor != null && decor.Attachment == DecorAttachment.Cintre;

            sb.Append("<b>OBJECT INFO</b>\n");
            sb.Append("<b>").Append(name).Append("</b>  (").Append(flown ? "flown, top centre" : "floor, bottom centre").Append(")\n\n");

            // Position : point d'origine, depuis le point zéro, dans le repère du plan (comme le CSV)
            Vector3 stagePosition = decor != null ? decor.OriginStagePosition : t.localPosition;
            Vector3 plan = StagePlan.ToPlan(stagePosition);
            sb.Append("<b>Position</b> from zero point\n");
            sb.Append("  X ").Append(Meters(plan.x)).Append("   (+ right / - left)\n");
            sb.Append("  Y ").Append(Meters(plan.y)).Append("   (+ far / - near)\n");
            sb.Append("  Z ").Append(Meters(plan.z)).Append("   (above floor)\n");

            Vector3 angles = StagePlan.ToPlanAngles(t.localRotation);
            sb.Append("<b>Rotation</b> ").Append(Number(angles.z, 1)).Append(" deg");
            if (Mathf.Abs(angles.x) > 0.05f || Mathf.Abs(angles.y) > 0.05f)
            {
                sb.Append("   tilt X ").Append(Number(angles.x, 1)).Append("  tilt Y ").Append(Number(angles.y, 1));
            }
            sb.Append("\n\n");

            // Dimensions le long des axes de l'objet (largeur = X, profondeur = Z, hauteur = Y)
            if (decor != null)
            {
                Vector3 size = decor.StageSize;
                float width = Mathf.Abs(size.x), depth = Mathf.Abs(size.z), height = Mathf.Abs(size.y);
                sb.Append("<b>Size</b>  W ").Append(Number(width, 2)).Append(" x D ").Append(Number(depth, 2))
                  .Append(" x H ").Append(Number(height, 2)).Append(" m\n");
                sb.Append("<b>Height</b> ").Append(Meters(height));
                if (decor.TryGetStageBounds(out var bounds))
                {
                    sb.Append("   (top at ").Append(Meters(bounds.max.y)).Append(" above floor)");
                }
                sb.Append('\n');
                sb.Append("<b>Volume</b> ").Append(Number(width * depth * height, 2)).Append(" m³  (bounding box)");
            }
            else
            {
                Vector3 size = target.GetWorldBounds().size;
                sb.Append("<b>Height</b> ").Append(Meters(size.y)).Append('\n');
                sb.Append("<b>Volume</b> ").Append(Number(size.x * size.y * size.z, 2)).Append(" m³  (bounding box)");
            }

            return sb.ToString();
        }

        static string Meters(float value) => Number(value, 2) + " m";

        static string Number(float value, int decimals)
        {
            float rounded = (float)System.Math.Round(value, decimals);
            if (rounded == 0f) rounded = 0f; // pas de « -0 »
            return rounded.ToString("F" + decimals, CultureInfo.InvariantCulture);
        }

        static ObjectInfoPanel Build()
        {
            // 46 x 40 cm (1 unité = 1 mm)
            var panel = Create<ObjectInfoPanel>("ObjectInfoPanel", new Vector2(460f, 400f), 0.001f, clickable: false);
            panel._text = panel.AddText("", 20f, 16f, 420f, 370f, 20f);
            panel._text.richText = true;
            return panel;
        }
    }
}
