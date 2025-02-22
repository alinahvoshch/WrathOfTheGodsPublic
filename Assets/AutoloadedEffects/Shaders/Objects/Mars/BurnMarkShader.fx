sampler baseTexture : register(s0);

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float seed = sampleColor.r;
    float lifetimeRatio = sampleColor.g;
    float noise = tex2D(baseTexture, coords * 1.3 + seed);
    
    float distanceFromCenter = distance(coords, 0.5);
    float opacity = smoothstep(0.5, 0.35, distanceFromCenter + noise * 0.3 + lifetimeRatio * 0.25) * sampleColor.a;
    
    return tex2D(baseTexture, coords * 0.6 + seed) * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}