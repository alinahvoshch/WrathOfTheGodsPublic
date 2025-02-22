sampler baseTexture : register(s0);
sampler glowmaskTexture : register(s1);

float3 ColorDodge(float3 a, float3 b)
{
    return a / (1 - b);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(baseTexture, coords);
    return float4(ColorDodge(color.rgb, tex2D(glowmaskTexture, coords).rgb * sampleColor.rgb), 1) * color.a * sampleColor.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}