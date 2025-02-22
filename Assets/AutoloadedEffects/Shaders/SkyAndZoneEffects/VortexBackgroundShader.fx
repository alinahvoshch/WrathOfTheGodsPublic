sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float time;
float holeGrowInterpolant;
float revealDimensionInterpolant;
float2 vortexCenter;
float2 textureSize;
float4 innerGlowColor;

float2 AspectRatioCorrect(float2 coords)
{
    return (coords - 0.5) * float2(textureSize.x / textureSize.y, 1) + 0.5;
}

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Center the vortex.
    coords = AspectRatioCorrect(coords);
    coords = (coords - vortexCenter) * 1.6 + vortexCenter;
    
    // Calulcate polar coordinates.
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distance(coords, 0.5));
    float distanceFromEdge = 0.5 - polar.y;
    
    // Use two polar noise calculations to calculate an offset angle, resulting in more harsh effects.
    float angleOffsetNoise = tex2D(noiseTexture, polar * float2(1, 2) + time * float2(0.25, -0.9)) + tex2D(noiseTexture, polar * float2(2, 3) + time * float2(0.05, -0.1));
    float angle = angleOffsetNoise * min(pow(polar.y, 1.25) * 3.35, 1.5) + polar.y * 20 - time * 10;
    
    // Rotate by the angle.
    coords = RotatedBy(coords - 0.5, angle) + 0.5;
    
    // Apply an inner glow.
    float innerGlow = smoothstep(0.2, 0.01, polar.y) * 2;
    
    // Combine everything together.
    float4 result = (tex2D(baseTexture, coords) * sampleColor + innerGlow * innerGlowColor) * (0.5 - distanceFromEdge) * 4.5;
    result.a = 1;
    
    // Make the inside of the rift open up based on the hole grow interpolant.
    result *= smoothstep(1, 1 + revealDimensionInterpolant * 2 + angleOffsetNoise, polar.y / holeGrowInterpolant);
    
    return result;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}