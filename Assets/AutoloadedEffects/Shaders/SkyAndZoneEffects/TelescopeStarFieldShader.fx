sampler baseTexture : register(s0);
sampler starNoiseTexture1 : register(s1);

float globalTime;
float twinkleSpeed;
float2 scrollOffset;
float2 pixelSize;

float hash12(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 30000);
}

float CalculateStarBrightness(float2 coords, float cutoffThreshold)
{
    float brightness = hash12(coords);
    if (brightness >= cutoffThreshold)
    {
        brightness = pow((brightness - cutoffThreshold) / (1 - cutoffThreshold), 26);
        float twinkle = cos(globalTime * twinkleSpeed + brightness * 150) * 0.5;
        brightness *= (1 + twinkle);
    }
    else
        brightness = 0.0;
    
    return brightness;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords -= scrollOffset;
    coords = round(coords * pixelSize) / pixelSize;
    float brightness = CalculateStarBrightness(coords * 10, 0.95);
    return brightness;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
