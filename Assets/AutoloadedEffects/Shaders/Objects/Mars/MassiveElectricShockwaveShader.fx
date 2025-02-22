sampler baseTexture : register(s0);
sampler electicNoiseTexture : register(s1);

float globalTime;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate polar coordinates for calculations below.
    float distanceFromCenter = distance(coords, 0.5);
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distanceFromCenter);
    
    float edge = 0.4;
    
    // Calculate noise for the purpose of offseting the edge distance calculations, resulting in a more harsh, unclean boundary between edge.
    float distanceNoise = tex2D(electicNoiseTexture, polar * float2(1, 0.9) + float2(0, globalTime * 0.1));
    float glowDistanceInfluence = tex2D(electicNoiseTexture, polar * 2 + globalTime * 0.2) * 0.02;
    
    // Calculate the distance from the edge of the color.
    float distanceFromEdge = distance(distanceFromCenter + distanceNoise * 0.02, edge);
    
    float glowIntensityCoefficient = lerp(0.003, 0.09, sampleColor.a);
    float glowIntensityExponent = distanceFromEdge * 2 + 1.1;
    float4 glow = glowIntensityCoefficient / pow(max(0.00001, distanceFromEdge - glowDistanceInfluence), glowIntensityExponent);
    
    sampleColor.rgb += float3(-0.5, 0.9, 0.9) * distanceFromEdge;
    
    float4 color = clamp(glow * smoothstep(0.5, edge, distanceFromCenter), 0, sampleColor.a * 500);
    return saturate(color * sampleColor) * smoothstep(0, 0.15, sampleColor.a);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}