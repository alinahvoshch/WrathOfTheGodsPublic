sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float appearanceInterpolant;
float gradientCount;
float2 pixelationFactor;
float3 gradient[8];

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(frac(interpolant) * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords = floor(coords / pixelationFactor) * pixelationFactor;
    
    float4 color = tex2D(baseTexture, coords);
    float luminosity = dot(color.rgb, float3(0.3, 0.6, 0.1));
    
    float opacity = smoothstep(-0.32, 0, distance(luminosity, 1));
    
    float yPositionNoise = tex2D(noiseTexture, coords * 2);
    float yPositionInterpolant = smoothstep(0.1, 0, (1 - coords.y) - appearanceInterpolant + yPositionNoise * 0.2);
    float gradientMapInterpolant = (1.3 - color.b) * yPositionInterpolant * appearanceInterpolant;
    
    float3 gradientifiedColor = PaletteLerp(luminosity * 2 + globalTime * 0.18) * 1.5;
    gradientifiedColor *= smoothstep(0.15, 0.27, dot(color.rgb, 0.333));
    
    color = float4(lerp(color.rgb, gradientifiedColor, gradientMapInterpolant), 1) * color.a;
    
    return color * sampleColor * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}