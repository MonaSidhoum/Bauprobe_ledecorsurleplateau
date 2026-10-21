// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.UI;

namespace MRUtilityKitSample.FindFloorZone
{
    /// <summary>
    /// UI component that manages a slider that follows a target object and changes color based on slider value.
    /// Used for displaying stamina or progress indicators in world space.
    /// </summary>
    [MetaCodeSample("MRUK-FindFloorZone")]
    public class SliderUI : MonoBehaviour
    {
        private const float UpdateFrequency = 0.016f; // ~60 FPS update rate
        private const float ColorChangeThreshold = 0.01f; // Only update color if change is significant

        [SerializeField] private Vector3 Offset = Vector3.zero;

        [SerializeField] private Transform TargetObject;

        [SerializeField] private Color ColorFull = Color.green;
        [SerializeField] private Color ColorEmpty = Color.red;
        [SerializeField] private Image FillImage;

        private Slider _slider;
        private Camera _camera;
        private Transform _thisTransform;
        private Transform _cameraTransform;

        // Performance optimization: Cache values and reduce update frequency
        private float _lastSliderValue = -1f;
        private float _updateTimer;
        private Vector3 _cachedOffset;

        private void Start()
        {
            _camera = Camera.main;
            _slider = GetComponent<Slider>();
            _thisTransform = transform;
            if (_camera != null)
            {
                _cameraTransform = _camera.transform;
            }

            _cachedOffset = Offset;
        }

        private void Update()
        {
            // Reduce update frequency for better performance
            _updateTimer += Time.deltaTime;
            if (_updateTimer >= UpdateFrequency)
            {
                UpdatePosition();
                UpdateColor();
                _updateTimer = 0f;
            }
        }

        private void UpdatePosition()
        {
            // Cache transform operations to avoid repeated property access
            var targetPosition = TargetObject.position;
            var cameraRotation = _cameraTransform.rotation;

            _thisTransform.position = targetPosition + cameraRotation * _cachedOffset;
            _thisTransform.rotation = cameraRotation;
        }

        private void UpdateColor()
        {
            // Only update color if slider value changed significantly
            var currentSliderValue = _slider.value;
            if (!(Mathf.Abs(currentSliderValue - _lastSliderValue) > ColorChangeThreshold))
            {
                return;
            }

            var newColor = Color.Lerp(ColorEmpty, ColorFull, currentSliderValue);
            FillImage.color = newColor;
            _lastSliderValue = currentSliderValue;
        }
    }
}
