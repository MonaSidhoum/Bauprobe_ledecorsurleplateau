using System.Globalization;
using TMPro;
using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Pavé numérique affiché à la main gauche pendant le mode Scale (en anglais) :
    /// on tape un pourcentage de la taille D'ORIGINE du décor (celle de son prefab) puis APPLY.
    /// Ex. 100 = retour à la taille d'origine, 150 = 1,5 fois la taille d'origine, 50 = moitié.
    /// Le Scale au joystick marche toujours en même temps. Gâchette ailleurs = valider, B = annuler (y compris
    /// ce qui a été appliqué avec le pavé).
    /// </summary>
    public class ScaleKeypad : HandPanel
    {
        const int MaxDigits = 7;

        static ScaleKeypad s_instance;

        InteractableObject _target;
        TextMeshProUGUI _display;
        TextMeshProUGUI _status;
        string _input = "";

        public static void Show(InteractableObject target)
        {
            if (target == null) return;
            if (s_instance == null) s_instance = Build();

            s_instance._target = target;
            s_instance._input = "";
            s_instance._status.text = "";
            s_instance.gameObject.SetActive(true);
            s_instance.UpdateDisplay();
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
            UpdateDisplay();
        }

        void OnDestroy()
        {
            if (s_instance == this) s_instance = null;
        }

        void Type(string key)
        {
            if (key == "." && _input.Contains(".")) return;
            if (_input.Length >= MaxDigits) return;
            _input += key;
            _status.text = "";
        }

        void Backspace()
        {
            if (_input.Length > 0) _input = _input.Substring(0, _input.Length - 1);
            _status.text = "";
        }

        void Apply()
        {
            if (_target == null) return;
            if (!TryParsePercent(_input, out float percent))
            {
                _status.text = "<color=#FF8080>Type a value above 0</color>";
                return;
            }

            _target.ApplyScalePercentOfOriginal(percent);
            _status.text = "Applied: " + percent.ToString("0.##", CultureInfo.InvariantCulture) + " % of original";
            _input = "";
        }

        /// <summary>Lit un pourcentage (point ou virgule acceptés). Vrai seulement s'il est supérieur à 0.</summary>
        public static bool TryParsePercent(string text, out float percent)
        {
            percent = 0f;
            if (string.IsNullOrEmpty(text)) return false;
            return float.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out percent)
                   && percent > 0f && !float.IsInfinity(percent);
        }

        void UpdateDisplay()
        {
            _display.text = (_input.Length > 0 ? _input : "<color=#888888>___</color>") + " %";
        }

        static ScaleKeypad Build()
        {
            // 36 x 52 cm (1 unité = 1 mm). Les textes sont en haut : le bas du panneau peut passer
            // près du menu général sans rien cacher d'important.
            var panel = Create<ScaleKeypad>("ScaleKeypad", new Vector2(360f, 520f), 0.001f, clickable: true);

            panel.AddText("<b>SCALE</b>  % of original size (100 = original)
" +
                          "<size=15>Joystick still scales.  Trigger elsewhere = done,  B = cancel</size>",
                          20f, 10f, 320f, 70f, 20f);
            panel._display = panel.AddText("", 20f, 80f, 320f, 50f, 38f, TextAlignmentOptions.Right);
            panel._status = panel.AddText("", 20f, 132f, 320f, 32f, 18f, TextAlignmentOptions.Center);

            // Touches : 7 8 9 / 4 5 6 / 1 2 3 / . 0 ⌫
            string[] keys = { "7", "8", "9", "4", "5", "6", "1", "2", "3", ".", "0", "<" };
            const float size = 96f, height = 60f, gap = 10f, left = 27f, top = 168f;
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                float x = left + (i % 3) * (size + gap);
                float y = top + (i / 3) * (height + gap);
                if (key == "<") panel.AddButton("DEL", x, y, size, height, panel.Backspace, fontSize: 24f);
                else panel.AddButton(key, x, y, size, height, () => panel.Type(key), fontSize: 32f);
            }

            float bottom = top + 4 * (height + gap);
            panel.AddButton("APPLY", left, bottom, 3 * size + 2 * gap, 60f, panel.Apply, accent: true);
            return panel;
        }
    }
}
