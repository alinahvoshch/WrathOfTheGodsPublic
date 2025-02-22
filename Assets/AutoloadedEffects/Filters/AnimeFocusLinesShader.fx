sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float intensity;
float opacity;
float globalTime;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 scroll = globalTime * float2(1, 0.3);
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distance(coords, 0.5));
    
    // Use two sets of noise with opposite scroll offsets, averaged together, to compose the overall line base.
    float noise = (tex2D(noiseTexture, polar * float2(24, 0.15) + scroll) + tex2D(noiseTexture, polar * float2(32, 0.15) - scroll)) * 0.5;
    
    // Cut off the noise such that only the brightest values remain, stripping the noise of its continuum and reducing it to lines.
    float lines = pow(smoothstep(0.5, 0.85, noise), 1.75);
    
    // Calculate a distance fade. This is used to make the lines translucent as they approach the center of the screen, so as to not obstruct the player's view too much.
    float distanceFade = pow(smoothstep(0.15, 0.55, polar.y), 4);
    
    float lineSubtraction = -lines * intensity * opacity * distanceFade * 0.133;
    
    return tex2D(baseTexture, coords) + lineSubtraction;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}