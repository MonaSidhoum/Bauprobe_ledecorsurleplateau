// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.VirtualHome
{
    /// <summary>
    /// A simple billboard component that makes the GameObject always face the main camera.
    /// The billboard rotates to look at the camera position each frame.
    /// </summary>
    [MetaCodeSample("MRUKSample-VirtualHome")]
    public class SimpleBillboard : MonoBehaviour
    {
        private Camera _mainCamera;

        /// <summary>
        /// Initializes the billboard by caching a reference to the main camera.
        /// </summary>
        private void Start()
        {
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// Updates the billboard rotation to face the main camera each frame.
        /// </summary>
        private void Update()
        {
            var direction = transform.position - _mainCamera.transform.position;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = rotation;
        }
    }
}
