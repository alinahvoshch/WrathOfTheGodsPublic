sampler baseTexture : register(s0);

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(baseTexture, coords) * sampleColor;
    float gray = dot(color.rgb, float3(0.3, 0.6, 0.1));
    return float4(gray, gray, gray, 1) * color.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}