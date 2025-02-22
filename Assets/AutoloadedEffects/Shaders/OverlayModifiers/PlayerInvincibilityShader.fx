sampler baseTexture : register(s0);

float globalTime;
float gradientCount;
float fadeInInterpolant;
float2 baseTextureSize;
float4 frame;
float3 gradient[8];

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(frac(interpolant) * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 framedCoords = (coords * baseTextureSize - frame.xy) / frame.zw;
    
    float4 color = tex2D(baseTexture, coords);
    float luminosity = dot(color.rgb, float3(0.3, 0.6, 0.1));
    float4 mappedColor = float4(lerp(color.rgb, PaletteLerp(luminosity * 1.3), 1.3 - color.g), 1) * color.a * 1.25;
    
    float glow = (0.5 - distance(fadeInInterpolant, 0.5)) * 2.3;
    
    return lerp(color, mappedColor, fadeInInterpolant * 0.67) * sampleColor + glow * color.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}