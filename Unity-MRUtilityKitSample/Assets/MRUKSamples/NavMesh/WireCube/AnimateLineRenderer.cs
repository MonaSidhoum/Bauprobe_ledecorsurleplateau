// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;

namespace MRUtilityKitSample.NavMesh
{
    [MetaCodeSample("MRUK-NavMesh")]
    public class AnimateLineRenderer : MonoBehaviour
    {
        private static readonly int ProximityBased = Shader.PropertyToID("_ProximityBased");
        private static readonly int TargetPosition = Shader.PropertyToID("_TargetPosition");
        [Range(0, 1)] public float Evolution = 1;
        public bool ProximityBasedTransparent;
        public Transform ProximityTarget;
        private LineRenderer[] _lineRenderers;
        private Color _originalCol;

        private void Start()
        {
            _lineRenderers = GetComponentsInChildren<LineRenderer>();
            _originalCol = _lineRenderers[0].material.color;
        }

        private void Update()
        {
            foreach (var lineRenderer in _lineRenderers)
            {
                lineRenderer.SetPosition(1, new Vector3(0, 0, Evolution));
                if (Evolution < .1f)
                {
                    lineRenderer.material.color = new Color(_originalCol.r * Evolution * 10,
                        _originalCol.g * Evolution * 10,
                        _originalCol.b * Evolution * 10, Evolution * 10);
                }
                else
                {
                    lineRenderer.material.color = _originalCol;
                }

                if (ProximityBasedTransparent)
                {
                    if (ProximityTarget == null)
                    {
                        Debug.LogError("Proximity target not set");
                        return;
                    }

                    lineRenderer.material.SetInt(ProximityBased, 1);
                    lineRenderer.material.SetVector(TargetPosition, ProximityTarget.position);
                }
            }
        }
    }
}
