// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.QRCodeDetection
{
    /// <summary>
    /// Makes a Canvas face the camera by continuously updating its rotation to look at the camera.
    /// This component is useful for QR code detection UI elements that need to remain visible to the user.
    /// </summary>
    [MetaCodeSample("MRUKSample-QRCodeDetection")]
    [RequireComponent(typeof(Canvas))]
    public class QRCodeFaceCamera : MonoBehaviour
    {
        private Canvas _canvas;

        private void Start()
        {
            _canvas = GetComponent<Canvas>();
            if (Camera.main != null)
            {
                _canvas.worldCamera = Camera.main;
            }
            else
            {
                Debug.LogWarning("Camera.main is null. Canvas world camera will not be set.", this);
            }
        }

        private void Update()
        {
            if (_canvas && _canvas.worldCamera)
            {
                // Calculate rotation to make the canvas face the camera by looking from canvas position toward camera position
                transform.rotation =
                    Quaternion.LookRotation(transform.position - _canvas.worldCamera.transform.position);
            }
        }
    }
}
