Shader "Unlit/Stancil"
{
    Properties
    {
        [IntRange] _StencilRef ("Stencil Reference", Range(0,255)) = 0
    }
    SubShader
    {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry-1"
            }

        Pass
        {
            Blend Zero One
            Zwrite Off

            Stencil
            {
                Ref [_StencilRef]
                Comp always
                Pass replace
                //Fail Keep
            }
        }
    }
}
