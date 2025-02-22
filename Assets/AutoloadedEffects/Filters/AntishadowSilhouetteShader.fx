sampler baseTexture : register(s0);

float3 silhouetteColor;
float3 foregroundColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(baseTexture, coords);
    bool useSpecialColor = color.r <= 0.001 && color.g >= 0.999 && color.b <= 0.001;
    return lerp(float4(silhouetteColor, 1), float4(foregroundColor, 1), useSpecialColor);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}