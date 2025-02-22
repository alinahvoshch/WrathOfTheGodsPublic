sampler bloodTexture : register(s1);
sampler lowLevelDetailTexture : register(s2);

float globalTime;
float cutoffStartY;
float cutoffEndY;

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = sampleColor;
    
    // Apply subtractive blending to make the texture bias towards dark purples at points.
    float2 subtractiveNoiseCoords = (coords + float2(0, globalTime * 1.94)) * float2(1.15, 0.7);
    float subtractiveNoise = tex2D(lowLevelDetailTexture, subtractiveNoiseCoords);
    color.rgb -= pow(subtractiveNoise, 3) * float3(0.9, 0.5, 0.06) * 0.3;
    
    // Apply additive blending to add texture to the blood.
    // This uses a wave effect to make the motion feel a bit more dynamic.
    float offsetNoise = tex2D(lowLevelDetailTexture, coords);
    float2 additiveNoiseCoords = coords * float2(2.4, 0.8) + float2(cos(-coords.y * 9 + globalTime * 5.1) * 0.1, globalTime * 2.3);
    color.rgb += pow(1 - tex2D(bloodTexture, additiveNoiseCoords).r, 2.5) * float3(0.6, 0.1, 0.1);
    
    float opacity = smoothstep(coords.y * 0.9, 1, pow(QuadraticBump(coords.x), 3 - smoothstep(1, 0.6, coords.y) * 2)) * smoothstep(1, 0.95, coords.y);
    color.rgb -= smoothstep(0.85, 1, coords.y) * 0.3;
    color = floor(color * 12) / 12;
    
    color *= opacity;
    
    color *= smoothstep(cutoffStartY - 0.03, cutoffStartY, coords.y);
    color *= smoothstep(cutoffEndY + 0.03, cutoffEndY, coords.y);
    
    return color * sampleColor.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
