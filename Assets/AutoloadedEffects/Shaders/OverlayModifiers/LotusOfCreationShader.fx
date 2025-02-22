sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float appearanceInterpolant;
float gradientCount;
float3 gradient[8];

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(frac(interpolant) * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(baseTexture, coords);
    float luminosity = dot(color.rgb, float3(0.3, 0.6, 0.1));
    
    float opacity = smoothstep(-0.32, 0, distance(luminosity, 1) - (1 - appearanceInterpolant));
    
    color = float4(lerp(color.rgb, PaletteLerp(luminosity * 2 - globalTime * 0.4), 1.3 - color.b), 1) * color.a * 1.5;
    
    return color * sampleColor * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}