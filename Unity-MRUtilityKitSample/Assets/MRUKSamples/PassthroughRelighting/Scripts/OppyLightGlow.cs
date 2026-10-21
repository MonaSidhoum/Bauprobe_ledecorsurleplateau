// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.PassthroughRelighting
{
    [MetaCodeSample("MRUKSample-PassthroughRelighting")]
    public class OppyLightGlow : MonoBehaviour
    {
        private const string EmissionColorShaderPropertyName = "_EmissionColor";
        [SerializeField] private Material oppyMaterial;
        private readonly Color _glowColor = new(1, 0.8f, 0.3f);
        private readonly Color _noGlowColor = Color.black;

        public void SetGlowActive(bool active)
        {
            if (!active)
            {
                oppyMaterial.SetColor(EmissionColorShaderPropertyName, _noGlowColor);
            }
            else
            {
                oppyMaterial.SetColor(EmissionColorShaderPropertyName, _glowColor);
            }
        }
    }
}
