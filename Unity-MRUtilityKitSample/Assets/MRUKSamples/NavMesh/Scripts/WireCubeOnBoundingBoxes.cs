// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections;
using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;
using UnityEngine;

namespace MRUtilityKitSample.NavMesh
{
    [MetaCodeSample("MRUK-NavMesh")]
    public class WireCubeOnBoundingBoxes : MonoBehaviour
    {
        [SerializeField] private GameObject _wireCubePrefab;
        [SerializeField] private Transform _revealingTarget;
        private EffectMesh _effectMesh;

        /// <summary>
        /// Spawns wire cube visualizations on all bounding boxes in the scene.
        /// </summary>
        public void SpawnWireCubeOnBoundingBoxes()
        {
            StartCoroutine(SpawnWireCubeOnBoundingBoxesCoroutine());
        }

        private IEnumerator SpawnWireCubeOnBoundingBoxesCoroutine()
        {
            _effectMesh = GetComponent<EffectMesh>();
            yield return new WaitUntil(() => _effectMesh != null && _effectMesh.EffectMeshObjects.Count > 0);
            var bBox = _effectMesh.EffectMeshObjects;
            foreach (var box in bBox)
            {
                // Get the effect mesh object
                var effectMeshObj = box.Value;

                // Get position from the GameObject's transform
                var position = effectMeshObj.effectMeshGO.GetComponent<MeshRenderer>().bounds.center;
                // Get bounds from the mesh
                var bounds = effectMeshObj.mesh.bounds;
                var size = bounds.size;

                // Instantiate the wire cube prefab
                var wireCube = Instantiate(_wireCubePrefab, position, effectMeshObj.effectMeshGO.transform.rotation);

                // Scale the wire cube to match the mesh size
                wireCube.transform.localScale = size;
                var animateLineRenderer = wireCube.GetComponent<AnimateLineRenderer>();
                animateLineRenderer.ProximityBasedTransparent = true;
                animateLineRenderer.ProximityTarget = _revealingTarget;
            }
        }
    }
}
