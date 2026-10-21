// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;

using UnityEngine;


namespace Meta.XR.MRUtilityKitSamples.QRCodeDetection
{
    /// <summary>
    /// Visualizes 2D boundaries of trackable objects using a LineRenderer and positions a canvas relative to the boundary.
    /// This component creates a rectangular outline around the tracked object's plane boundary and maintains
    /// a canvas position that follows the boundary with a configurable offset.
    /// </summary>
    [MetaCodeSample("MRUKSample-QRCodeDetection")]
    public class Bounded2DVisualizer : MonoBehaviour
    {
        [SerializeField]
        LineRenderer _lineRenderer;
        [SerializeField]
        RectTransform _canvasRect;
        [SerializeField, Tooltip("(in Canvas-local units)")]
        Vector3 _canvasOffset = new(0f, -15f, 0f);

        MRUKTrackable _trackable;

        Rect _box;

        /// <summary>
        /// Initializes the visualizer with a trackable object to display its 2D boundary.
        /// </summary>
        /// <param name="trackable">The MRUKTrackable object whose boundary will be visualized.</param>
        public void Initialize(MRUKTrackable trackable)
        {
            _trackable = trackable;

            if (trackable.PlaneBoundary2D == null && trackable.PlaneRect == null)
            {
                Debug.LogWarning($"{trackable} is missing a plane component.");
            }
            else
            {
                UpdateBoundingBox();
            }
        }

        void Update()
        {
            if (!_trackable)
            {
                return;
            }

            UnityEngine.Assertions.Assert.IsTrue(_trackable.PlaneRect.HasValue);
            _box = _trackable.PlaneRect.Value;

            UpdateBoundingBox();

            if (!_canvasRect)
            {
                return;
            }

            // Position the canvas relative to the bounding box with the configured offset,
            // scaling the offset by the canvas's local scale to maintain consistent positioning
            _canvasRect.localPosition = new Vector3(
                x: _box.center.x + _canvasOffset.x * _canvasRect.localScale.x,
                y: _box.yMin + _canvasOffset.y * _canvasRect.localScale.y,
                z: _canvasOffset.z * _canvasRect.localScale.z
            );
        }

        void UpdateBoundingBox()
        {
            // Create a rectangular boundary visualization by setting up 4 corner positions
            _lineRenderer.positionCount = 4;
            _lineRenderer.SetPosition(0, new Vector3(_box.x, _box.y, 0));
            _lineRenderer.SetPosition(1, new Vector3(_box.x + _box.width, _box.y, 0));
            _lineRenderer.SetPosition(2, new Vector3(_box.x + _box.width, _box.y + _box.height, 0));
            _lineRenderer.SetPosition(3, new Vector3(_box.x, _box.y + _box.height, 0));
        }
    }
}
