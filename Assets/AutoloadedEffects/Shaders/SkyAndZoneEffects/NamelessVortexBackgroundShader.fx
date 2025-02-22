sampler staticTexture : register(s0);

float time;
float gradientCount;
float streakUndermineFactor;
float holeGrowInterpolant;
float revealDimensionInterpolant;
float2 vortexCenter;
float2 textureSize;
float4 gradient[20];
float4 backgroundColor;
bool subtractive;

float2 AspectRatioCorrect(float2 coords)
{
    return (coords - 0.5) * float2(textureSize.x / textureSize.y, 1) + 0.5;
}

float4 PaletteLerp(float interpolant)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords = lerp(coords, vortexCenter, -distance(vortexCenter, 0.5) * 12);
    
    // Calculate polar coordinates.
    // The radius part is significantly undercut to help ensure that sampled coordinates are more line-like as they travel, to help
    // give the appearance of fast-moving streaks.
    float distanceFromCenter = distance(AspectRatioCorrect(coords), AspectRatioCorrect(vortexCenter)) * 0.8;
    float2 polar = float2(atan2(coords.y - vortexCenter.y, coords.x - vortexCenter.x) / 6.283 + 0.5, distanceFromCenter * 0.1);
    
    // Apply a trippy swirl effect based on how far away the hole center is from the 0.5 point.
    polar.x += polar.y * distance(vortexCenter, 0.5) * (vortexCenter.x - 0.5) * -1100;
    
    // Calculate the streak color interpolant.
    // This uses a polar texture sample as the base, but is undermined by two separate texture samples.
    // This helps ensure that streaks aren't too numerous and that the result is easier on the eyes.
    float colorInterpolant = sqrt(tex2D(staticTexture, polar * float2(7, 6) + float2(0, time * -0.8)) - tex2D(staticTexture, polar * float2(1, 3) + float2(time * 0.4, time * -0.9)) * streakUndermineFactor);
    colorInterpolant -= tex2D(staticTexture, polar + float2(0, time * 0.1)) * streakUndermineFactor * (distanceFromCenter * 0.67 + 0.11) * 3;
    
    // Make the center dark.
    float centerFadeInterpolant = smoothstep(0.017, 0.06, distanceFromCenter * (1.01 - holeGrowInterpolant)) * smoothstep(0.017, 0.35, distanceFromCenter);
    colorInterpolant *= lerp(0.5, 1, centerFadeInterpolant);
    
    float4 color = PaletteLerp(pow(colorInterpolant, 1.75)) * smoothstep(0.4, 0.7, colorInterpolant.r) * sampleColor;
    
    // Combine everything together.
    float4 result = subtractive ? (1 - color) : (backgroundColor + color);
    result *= lerp(centerFadeInterpolant, 1, 1 - revealDimensionInterpolant);
    
    return result;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}