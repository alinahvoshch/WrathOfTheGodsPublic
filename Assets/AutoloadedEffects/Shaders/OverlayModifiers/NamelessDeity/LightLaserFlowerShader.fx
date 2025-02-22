sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float disappearanceInterpolant;
float gradientCount;
float hueOffset;
float3 gradient[5];

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(frac(interpolant) * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(baseTexture, coords);
    float luminosity = dot(baseColor.rgb, float3(0.3, 0.6, 0.1));
    
    float4 color = float4(PaletteLerp(luminosity * 0.93 + hueOffset), 1) * baseColor.a;
    color.a = 0;
    
    bool erasePixel = (1 - luminosity) + disappearanceInterpolant + tex2D(noiseTexture, coords * 1.5) * 0.6 >= 1.72;
    return color * sampleColor * (1 - erasePixel);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}