sampler kalisetFractal : register(s1);
sampler noiseTexture : register(s2);

float zoom;
float brightness;
float globalTime;
float colorChangeStrength1;
float colorChangeStrength2;
float detailIterations;
float expandInterpolant;
float2 parallaxOffset;
float2 avatarPosition;

float starGradientCount;
float3 starGradient[5];

float3 PaletteLerp(float interpolant, float3 gradient[5], float gradientCount)
{
    int startIndex = clamp(frac(interpolant) * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float4 result = 0;
    float volumetricLayerFade = 1;
    float distanceFromBottom = distance(coords.y, 1);
    float detailIterationsClamped = clamp(detailIterations, 1, 20);
    float2 offset = float2(0.375, 0.12);
    for (int i = 0; i < detailIterationsClamped; i++)
    {
        float time = globalTime * pow(volumetricLayerFade, 2) * 3;
        float2 p = (coords - offset) * zoom + offset;
        p.y += 9;

        // Perform scrolling behaviors. Each layer should scroll a bit slower than the previous one, to give an illusion of 3D.
        p += parallaxOffset / pow(i + 1, 0.3);
        p /= volumetricLayerFade;

        float totalChange = tex2D(kalisetFractal, p);
        float layerColorInterpolant = tex2D(noiseTexture, p) * 3;
        float4 layerColor = float4(PaletteLerp(layerColorInterpolant, starGradient, starGradientCount), 1);
        result += layerColor * totalChange * volumetricLayerFade;

        // Make the next layer exponentially weaker in intensity.
        volumetricLayerFade *= 0.89;
    }
    
    float distanceFromAvatar = distance(position.xy, avatarPosition);
    float fadeOut = smoothstep(11600, 8000, distanceFromAvatar / expandInterpolant);
    
    float4 combinedColor = saturate(result * sampleColor * brightness * 0.007);
    
    // Prevent isolated greens from appearing.
    combinedColor.g -= combinedColor.g * (1 - (combinedColor.b + combinedColor.r) * 0.5) * 0.35;
    
    return float4(0, 0, 0, 1) + pow(combinedColor, 4 + (1 - fadeOut) * 8) * fadeOut;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
