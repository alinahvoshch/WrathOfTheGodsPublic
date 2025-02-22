sampler baseTexture : register(s0);
sampler maskTarget : register(s1);

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 result = tex2D(baseTexture, coords);
    float4 maskData = tex2D(maskTarget, coords);
    result = lerp(result, maskData, smoothstep(0.9, 1, result.a));
    
    return result * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}