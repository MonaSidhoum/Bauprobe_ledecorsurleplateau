// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using TMPro;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.PassthroughRelighting
{
    [MetaCodeSample("MRUKSample-PassthroughRelighting")]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SelectableText : MonoBehaviour
    {
        [SerializeField] private bool SelectedOnToggleOn;
        private TextMeshProUGUI selectedText;
        private Color selectedColor = Color.white;
        private Color deselectedColor = new Color(0.3f, 0.3f, 0.3f);

        void Awake()
        {
            selectedText = GetComponent<TextMeshProUGUI>();
        }

        public void SetSelected(bool selected)
        {
            selectedText.color = selected == SelectedOnToggleOn ? selectedColor
                                                                : deselectedColor;
        }
    }
}
