sampler fireNoiseTexture : register(s1);
sampler edgeAccentTexture : register(s2);

float time;
float explosionShapeIrregularity;
float lifetimeRatio;
float4 accentColor;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    float distanceFromCenter = distance(coords, 0.5);
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distanceFromCenter);
    
    // Apply noise to the distance-from-center calculation, to turn the perfect circle into an irregularly shaped explosion.
    distanceFromCenter += tex2D(edgeAccentTexture, polar + time * float2(0.1, -0.8)) * explosionShapeIrregularity;
    
    // Determine how much the inner part of the explosion should glow.
    float innerGlow = lerp(0.03, 0.4, tex2D(edgeAccentTexture, polar + float2(0, -time)));    
    float4 color = sampleColor + innerGlow / distanceFromCenter;
    
    // Apply an accent color based on noise.
    color += accentColor * tex2D(fireNoiseTexture, polar * float2(2, 1) + time * float2(-0.1, -0.9)) * lifetimeRatio * 1.6;
    
    float fadeFromWithin = smoothstep(-0.15, 0, distanceFromCenter - pow(lifetimeRatio, 2.5) * 0.65);
    float edgeFade = smoothstep(0.5, 0.42, distanceFromCenter);
    
    return color * fadeFromWithin * edgeFade;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}