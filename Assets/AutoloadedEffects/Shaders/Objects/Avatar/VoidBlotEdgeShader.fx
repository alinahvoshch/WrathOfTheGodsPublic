sampler noiseGrainTexture : register(s1);
sampler noiseRingTexture : register(s2);

float globalTime;
float identity;
float3 edgeColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate glow and noise related values.
    float distanceFromCenter = distance(coords, 0.5);
    float noise = tex2D(noiseRingTexture, coords * 0.23);
    float edgeNoise = tex2D(noiseRingTexture, coords * 0.09 + globalTime * 0.05 + identity) - 0.7;
    float edgeGlowOpacity = pow(0.02 / distance(distanceFromCenter - edgeNoise * 0.091, 0.38) * sampleColor.a, 3);
    
    // Combine things together.
    float edgeCutoffOpacity = smoothstep(0.5, 0.49, distanceFromCenter);
    float4 result = noise * edgeCutoffOpacity * float4(edgeColor, 1) * clamp(edgeGlowOpacity, 0, 3);
    
    return result * sampleColor.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}