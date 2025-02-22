sampler screenTexture : register(s0);
sampler tentMaskTexture : register(s1);
sampler outsideTexture : register(s2);

float darkness;
float2 screenOffset;
float2 zoom;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(screenTexture, coords);
    float4 maskColor = tex2D(tentMaskTexture, (coords - 0.5) / zoom + 0.5 + screenOffset);
    return lerp(baseColor, tex2D(outsideTexture, coords), (1 - any(maskColor)) * darkness);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
