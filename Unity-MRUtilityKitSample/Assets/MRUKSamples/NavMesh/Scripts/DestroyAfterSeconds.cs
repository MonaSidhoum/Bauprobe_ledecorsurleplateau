// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;

namespace MRUtilityKitSample.NavMesh
{
    [MetaCodeSample("MRUK-NavMesh")]
    public class DestroyAfterSeconds : MonoBehaviour
    {
        [SerializeField] public float Seconds = 1f;

        private void Start()
        {
            Invoke("DestroySelf", Seconds);
        }

        private void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
