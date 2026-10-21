// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.VirtualHome
{
    [MetaCodeSample("MRUKSample-VirtualHome")]
    public class StickToVertex : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private int _indexID;

        private void Update()
        {
            if (_target == null)
            {
                return;
            }

            var meshFilter = _target.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                return;
            }

            var mesh = meshFilter.sharedMesh;
            if (_indexID < 0 || _indexID >= mesh.vertexCount)
            {
                return;
            }

            var localVertexPosition = mesh.vertices[_indexID];
            var worldVertexPosition = _target.TransformPoint(localVertexPosition);

            transform.position = worldVertexPosition;

            if (transform.parent != null)
            {
                var parentScale = transform.parent.lossyScale;
                if (parentScale.x != 0 && parentScale.y != 0 && parentScale.z != 0)
                {
                    transform.localScale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f / parentScale.z);
                }
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }
    }
}
