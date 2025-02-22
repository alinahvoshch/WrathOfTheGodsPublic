sampler baseTexture : register(s0);

float blurPower;
float distortionPower;
float pulseTimer;
float2 distortionCenter;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 distortionOffset = (distortionCenter - coords) * (sin(pulseTimer) * 0.55) * distortionPower;
    float offsetLength = length(distortionOffset);
    
    // Ensure that the offset does not exceed a certain intensity, to prevent it from being ridiculous due to the player
    // running far away from the source.
    if (offsetLength > 0.02)
        distortionOffset = 0.02 * distortionOffset / offsetLength;
    
    // Apply radial blur effects.
    float4 color = 0;
    for (int i = 0; i < 8; i++)
        color += tex2D(baseTexture, coords + distortionOffset + distortionOffset * i * blurPower * 0.15) * 0.125;
            
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}