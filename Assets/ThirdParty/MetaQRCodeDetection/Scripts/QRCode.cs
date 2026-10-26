// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;

using System.Linq;

using TMPro;

using UnityEngine;


namespace Meta.XR.MRUtilityKitSamples.QRCodeDetection
{
    [MetaCodeSample("MRUKSample-QRCodeDetection")]
    public sealed class QRCode : MonoBehaviour
    {
        public string PayloadText => _text.text;

        [SerializeField]
        TMP_Text _text;

        [SerializeField]
        TMP_Text _trackingStateText;

        [SerializeField]
        RectTransform _background;

        MRUKTrackable _trackable;

        public void Initialize(MRUKTrackable trackable)
        {
            if (trackable.MarkerPayloadString is { } str)
            {
                _text.text = $"\"{str}\"";
            }
            else if (trackable.MarkerPayloadBytes is { } bytes)
            {
                _text.text = $"Binary(data=[{string.Join(" ", bytes.Take(16).Select(b => $"{b:x02}"))}{(bytes.Length > 16 ? " ..." : "")}], length={bytes.Length})";
            }
            else
            {
                _text.text = "(no payload)";
            }

            _trackable = trackable;
            SetTrackingStateText();

            if (!_background)
            {
                return;
            }

            _text.ForceMeshUpdate();

            var bounds = _text.textBounds;
            _background.position = _text.transform.TransformPoint(bounds.center);

            var size = bounds.size;
            size.x += 16f;
            size.y += 16f;
            _background.sizeDelta = size;
        }

        void Update() => SetTrackingStateText();

        void SetTrackingStateText() => _trackingStateText.text = _trackable
            ? _trackable.IsTracked ? "Tracked" : "Untracked"
            : "(none)";
    }
}
