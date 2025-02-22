sampler baseTexture : register(s0);
sampler barOffsetNoiseTexture : register(s1);

float barOffsetMax;
float intensity;
float2 impactPosition;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Calculate in advance the blotch fadeoff.
    // This will match the blotch shader dissipate in intensity based on time and distance from the impact position.
    float distanceFromImpact = distance(impactPosition, position.xy);
    
    // Offset bars on the screen at random.
    float snappedYPosition = floor(coords.y * 23) / 23;
    float barOffset = tex2D(barOffsetNoiseTexture, float2(snappedYPosition, 0.3)) * smoothstep(0.8, 1, intensity) * barOffsetMax;
    coords.x = frac(coords.x + barOffset);
    
    // Apply chromatic aberration effects, separating the screen into bands of yellow and blue.
    float offset = intensity * 0.004;
    float4 leftColor = tex2D(baseTexture, coords + float2(-1, 0) * offset);
    float4 rightColor = tex2D(baseTexture, coords + float2(1, 0) * offset);
    float4 yellow = float4(leftColor.rg, 0, 1);
    float4 blue = float4(0, 0, rightColor.b, 1);
    float4 chromaticAberrationColor = yellow + blue;
    
    // Combine everything together.
    return chromaticAberrationColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}