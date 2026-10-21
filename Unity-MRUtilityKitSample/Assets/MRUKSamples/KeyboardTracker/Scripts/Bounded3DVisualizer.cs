// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.KeyboardTracker
{
    /// <summary>
    /// Visualizes a 3D bounded volume from an MRUKTrackable using line renderers and transforms.
    /// This component renders the wireframe bounds of a trackable object and manages passthrough mode visualization.
    /// </summary>
    [MetaCodeSample("MRUKSample-KeyboardTracker")]
    public class Bounded3DVisualizer : MonoBehaviour
    {
        [SerializeField]
        LineRenderer _lineRenderer;

        /// <summary>
        /// Gets the transform to which the trackable's detected position and scale should be applied.
        /// This transform will be scaled to match the size of the bounded volume.
        /// </summary>
        [field: SerializeField]
        [Tooltip("The transform to which the keyboard's detected position and scale should be applied.")]
        public Transform BoxTransform
        {
            get; private set;
        }

        [SerializeField]
        GameObject _axes;

        GameObject _skyBox;

        MRUKTrackable _trackable;

        readonly HashSet<string> _logOnce = new();

        void LogOnce(string msg)
        {
            if (_logOnce.Add(msg))
            {
                Debug.Log(msg);
            }
        }

        /// <summary>
        /// Initializes the visualizer with a trackable object and skybox reference.
        /// Sets up the line renderer to display the wireframe bounds and configures the box transform scale.
        /// </summary>
        /// <param name="trackable">The MRUKTrackable object containing the volume bounds to visualize.</param>
        /// <param name="skybox">The skybox GameObject used to determine passthrough mode state.</param>
        /// <remarks>
        /// If the trackable has no VolumeBounds component, the initialization will be skipped and logged.
        /// The line renderer will be configured to draw a wireframe representation of the bounding box.
        /// </remarks>
        public void Initialize(MRUKTrackable trackable, GameObject skybox)
        {
            if (trackable == null)
                throw new ArgumentNullException(nameof(trackable));

            _skyBox = skybox;

            _trackable = trackable;

            if (_trackable.VolumeBounds == null)
            {
                LogOnce($"Trackable {_trackable} has no Bounded3D component. Ignoring.");
                return;
            }

            var box = _trackable.VolumeBounds.Value;
            LogOnce($"Bounded3D volume: {box}");

            if (_lineRenderer)
            {
                var min = -box.extents;
                _lineRenderer.positionCount = 10;

                _lineRenderer.SetPosition(0, min);
                _lineRenderer.SetPosition(1, min + new Vector3(box.size.x, 0, 0));
                _lineRenderer.SetPosition(2, min + new Vector3(box.size.x, box.size.y, 0));
                _lineRenderer.SetPosition(3, min + new Vector3(0, box.size.y, 0));

                _lineRenderer.SetPosition(4, min);
                _lineRenderer.SetPosition(5, min + new Vector3(0, 0, box.size.z));

                _lineRenderer.SetPosition(6, min + new Vector3(box.size.x, 0, box.size.z));
                _lineRenderer.SetPosition(7, min + new Vector3(box.size.x, box.size.y, box.size.z));
                _lineRenderer.SetPosition(8, min + new Vector3(0, box.size.y, box.size.z));
                _lineRenderer.SetPosition(9, min + new Vector3(0, 0, box.size.z));
            }

            if (BoxTransform)
            {
                BoxTransform.localScale = box.size;
            }
            else
            {
                Debug.LogWarning($"Property '{nameof(BoxTransform)}' not set;");
            }
        }

        /// <summary>
        /// Sets the passthrough mode for the visualizer, controlling the visibility of visual elements.
        /// In full passthrough mode, the box transform is hidden and axes are shown.
        /// </summary>
        /// <param name="isFullPassthrough">True to enable full passthrough mode (hide box, show axes), false otherwise.</param>
        public void SetPassthroughMode(bool isFullPassthrough)
        {
            if (BoxTransform)
            {
                BoxTransform.gameObject.SetActive(!isFullPassthrough);
            }

            if (_axes)
            {
                _axes.SetActive(isFullPassthrough);
            }
        }

        private void Update()
        {
            SetPassthroughMode(!_skyBox.activeInHierarchy);
        }
    }
}
