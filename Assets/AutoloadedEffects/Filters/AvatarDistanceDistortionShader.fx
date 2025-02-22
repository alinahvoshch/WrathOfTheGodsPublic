sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float distortionStartRadius;
float distortionRadiusIntensityRange;
float2 screenResolution;
float2 avatarPosition;
float2 zoom;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_POSITION) : COLOR0
{
    // Calculate the distance from the current pixel to the Avatar to determine how much distortion should be applied.
    float2 offsetFromAvatar = (position.xy - avatarPosition);
    float distanceFromAvatar = length(offsetFromAvatar) / zoom.x;
    float distortionIntensity = smoothstep(0, distortionRadiusIntensityRange, distanceFromAvatar - distortionStartRadius);
    
    // Use noise to calculate the offset of the current pixel.
    // This will be affected based on the distortion intensity from above.
    float offsetX = (tex2D(noiseTexture, coords * 6.1 + globalTime) + tex2D(noiseTexture, coords * 3.2)) * 0.5 - 0.5;
    float offsetY = (tex2D(noiseTexture, coords * 5.7) + tex2D(noiseTexture, coords * 3.4 - globalTime)) * 0.5 - 0.5;
    float2 distortionOffset = float2(offsetX, offsetY) * distortionIntensity * 0.1;
    
    float glowInterpolant = cos(tex2D(noiseTexture, coords * 2.4 + distortionOffset * 0.6) * 3.141 - globalTime * 5) * 0.5 + 0.5;
    float glow = lerp(0.1, 0.9, glowInterpolant);
    
    return tex2D(baseTexture, coords + distortionOffset) + float4(1, glow, glow, 0) * pow(distortionIntensity, 2);
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}