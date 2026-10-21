// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.FindMultiSpawn
{
    [MetaCodeSample("MRUKSample-FindMultiSpawn")]
    public class MuzzleLight : MonoBehaviour
    {
        [SerializeField] private Light muzzlelight;
        [SerializeField] private ParticleSystem part;

        // Start is called before the first frame update
        private void Start()
        {
            muzzlelight.enabled = false;
        }

        // Update is called once per frame
        private void Update()
        {
            muzzlelight.enabled = part.particleCount > 0;
        }
    }
}
