// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;

namespace MRUtilityKitSample.FindFloorZone
{
    /// <summary>
    /// Tutorial component that guides users through the initial fishing mechanics.
    /// Automatically destroys itself once the user has extended the fishing line sufficiently.
    /// </summary>
    [MetaCodeSample("MRUK-FindFloorZone")]
    public class FishingIntroTutorial : MonoBehaviour
    {
        [SerializeField] private FishingRod FishingRod;

        private void Start()
        {
            if (!FishingRod)
            {
                FishingRod = FindAnyObjectByType<FishingRod>();
            }
        }

        private void Update()
        {
            if (FishingRod._stringGiven > .2f)
            {
                Destroy(gameObject);
            }
        }
    }
}
