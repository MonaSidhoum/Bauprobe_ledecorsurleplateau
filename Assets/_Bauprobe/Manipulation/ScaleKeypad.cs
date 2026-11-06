using System.Globalization;
using TMPro;
using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Pavé numérique affiché à la main gauche pendant le mode Scale (en anglais) :
    /// on tape un pourcentage de la taille ACTUELLE puis APPLY. Ex. 150 = 1,5 fois plus grand, 50 = deux fois plus petit.
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

            _target.ApplyScaleFactor(percent / 100f);
            _status.text = "Applied: x" + (percent / 100f).ToString("0.###", CultureInfo.InvariantCulture);
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
            // 36 x 56 cm (1 unité = 1 mm)
            var panel = Create<ScaleKeypad>("ScaleKeypad", new Vector2(360f, 560f), 0.001f, clickable: true);

            panel.AddText("<b>SCALE</b>\n% of current size (150 = x1.5)", 20f, 12f, 320f, 70f, 22f);
            panel._display = panel.AddText("", 20f, 86f, 320f, 56f, 40f, TextAlignmentOptions.Right);

            // Touches : 7 8 9 / 4 5 6 / 1 2 3 / . 0 ⌫
            string[] keys = { "7", "8", "9", "4", "5", "6", "1", "2", "3", ".", "0", "<" };
            const float size = 96f, gap = 12f, left = 24f, top = 150f;
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                float x = left + (i % 3) * (size + gap);
                float y = top + (i / 3) * (70f + gap);
                if (key == "<") panel.AddButton("DEL", x, y, size, 70f, panel.Backspace, fontSize: 26f);
                else panel.AddButton(key, x, y, size, 70f, () => panel.Type(key), fontSize: 34f);
            }

            float bottom = top + 4 * (70f + gap);
            panel.AddButton("APPLY", left, bottom, 3 * size + 2 * gap, 64f, panel.Apply, accent: true);
            panel._status = panel.AddText("", 20f, bottom + 70f, 320f, 40f, 20f, TextAlignmentOptions.Center);
            panel.AddText("Joystick still scales.  Trigger elsewhere = done,  B = cancel",
                          20f, bottom + 104f, 320f, 50f, 16f, TextAlignmentOptions.Center);
            return panel;
        }
    }
}
