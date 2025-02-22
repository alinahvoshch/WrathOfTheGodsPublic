sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float hueExponent;
float gradientCount;
float3 gradient[5];

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = clamp(startIndex + 1, 0, gradientCount - 1);
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distance(coords, 0.5));
    
    float noise = tex2D(noiseTexture, polar * float2(1, 0.1));
    float hueBase = sin(globalTime * 1.78 + coords.x * 5 + coords.y + noise * 2) * 0.5 + 0.5;
    float hue = pow(hueBase, hueExponent);
    float4 color = float4(PaletteLerp(hue), 1);
    
    return color * tex2D(baseTexture, coords) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}