sampler baseTexture : register(s0);
sampler cosmicTexture : register(s1);
sampler samplingNoiseTexture : register(s2);
sampler waterMaskTexture : register(s3);
sampler waterSlopesMaskTexture : register(s4);

float globalTime;
float brightnessFactor;
float opacity;
float2 screenPosition;
float2 oldScreenPosition;
float2 targetSize;
float2 zoom;
float4 generalColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 screenOffset = (screenPosition - oldScreenPosition) / targetSize;
    float2 worldStableCoords = (coords - 0.5) / zoom + 0.5 + screenPosition / targetSize;
    float4 color = tex2D(baseTexture, coords);
    
    // Use FBM to calculate complex warping offsets in the noise texture that will be used to create the fog noise.
    float noiseAmplitude = 0.051;
    float noiseScrollTime = globalTime * 0.4;
    float2 noiseOffset = 0;
    float2 noiseZoom = 3.187;
    for (float i = 0; i < 2; i++)
    {
        float2 scrollOffset = float2(noiseScrollTime * 0.67 - i * 0.838, noiseScrollTime * 1.21 + i * 0.6125) * noiseZoom.y * 0.04;
        noiseOffset += (tex2D(samplingNoiseTexture, worldStableCoords * noiseZoom + scrollOffset) - 0.5) * noiseAmplitude;
        noiseZoom *= 2;
        noiseAmplitude *= 0.5;
    }
    
    // Combine samples from the cosmic texture for a final color.
    float4 cosmicColor1 = pow(tex2D(cosmicTexture, worldStableCoords * 3 + float2(0, globalTime * (noiseOffset.x * 0.0007 + 0.04)) + noiseOffset), 3) * 3;
    float4 cosmicColor2 = pow(tex2D(cosmicTexture, worldStableCoords * 4 + float2(0, globalTime * 0.06) - noiseOffset), 0.6) * float4(1.2, 0.6, 0.97, 1);
    float4 cosmicColor = (cosmicColor1 + cosmicColor2) * (1 + abs(noiseOffset.x) * 10) * generalColor * 0.67;
    
    float2 maskCoords = (coords - 0.5) / zoom + 0.5 + screenOffset;
    bool anyWater = tex2D(waterMaskTexture, maskCoords).a > 0 || tex2D(waterSlopesMaskTexture, maskCoords).a > 0;
    float biasTowardsWaterColor = saturate(anyWater) * length(cosmicColor) * brightnessFactor * 0.095;
    
    return lerp(color, cosmicColor, biasTowardsWaterColor * opacity);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}