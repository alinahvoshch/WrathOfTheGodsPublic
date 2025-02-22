sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float materializeInterpolant;
float2 pixelationFactor;

float Hash12(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 30000);
}

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 pixelatedCoords = floor(coords / pixelationFactor) * pixelationFactor;
    float noise = tex2D(noiseTexture, pixelatedCoords * float2(0, 10)) - 0.5;
    float2 barOffset = float2(noise, 0) * (1 - materializeInterpolant);
    
    float4 baseColor = tex2D(baseTexture, coords + barOffset) * sampleColor;
    float2 starCoords = floor(coords / pixelationFactor) * pixelationFactor;
    
    float localMaterializationInterpolant = baseColor.a * materializeInterpolant;
    
    float twinkle = cos(globalTime * 7 + Hash12(pixelatedCoords) * 20) * 0.5 + 0.5;
    
    return baseColor + twinkle * (1 - materializeInterpolant) * baseColor.a * 2;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}