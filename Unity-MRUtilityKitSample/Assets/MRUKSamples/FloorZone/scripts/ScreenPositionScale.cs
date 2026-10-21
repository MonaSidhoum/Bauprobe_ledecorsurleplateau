// Copyright (c) Meta Platforms, Inc. and affiliates.


using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.XR;

namespace MRUtilityKitSample.FindFloorZone
{
    /// <summary>
    /// Updates material properties based on XR viewport scale for proper screen-space effects.
    /// Fixes resolution-dependent shader effects in VR environments.
    /// </summary>
    [MetaCodeSample("MRUK-FindFloorZone")]
    public class ScreenPositionScale : MonoBehaviour
    {
        private const float UpdateFrequency = 0.1f; // Update every 100ms instead of every frame
        private const float ScaleChangeThreshold = 0.001f; // Only update if scale changed significantly
        private const string ResolutionMultiplierProperty = "_ResolutionMultiplier";

        private Material _material;
        private float _lastRenderViewportScale = -1f;
        private float _updateTimer;
        private int _resolutionMultiplierPropertyId;

        private void Start()
        {
            _material = GetComponent<Renderer>().material;
            _resolutionMultiplierPropertyId = Shader.PropertyToID(ResolutionMultiplierProperty);

            // Set initial value
            UpdateResolutionMultiplier();
        }

        private void Update()
        {
            _updateTimer += Time.deltaTime;
            if (!(_updateTimer >= UpdateFrequency))
            {
                return;
            }

            UpdateResolutionMultiplier();
            _updateTimer = 0f;
        }

        private void UpdateResolutionMultiplier()
        {
            var currentScale = XRSettings.renderViewportScale;

            // Only update material if the scale changed significantly
            if (!(Mathf.Abs(currentScale - _lastRenderViewportScale) > ScaleChangeThreshold))
            {
                return;
            }

            _material.SetFloat(_resolutionMultiplierPropertyId, currentScale);
            _lastRenderViewportScale = currentScale;
        }
    }
}
