sampler2D baseTexture : register(s0);
sampler2D crackTexture : register(s1);
sampler2D uvOffsetingNoiseTexture : register(s2);

float globalTime;
float identity;
float disintegrationFactor;
float2 pixelationFactor;
float2 scatterDirectionBias;

float3 ColorDodge(float3 top, float3 bottom, float interpolant)
{
    return lerp(bottom, top / (1.001 - saturate(bottom)), interpolant);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Pixelate coords.
    coords = round(coords / pixelationFactor) * pixelationFactor;
    
    // Calculate values for the warp noise.
    float warpNoise = tex2D(uvOffsetingNoiseTexture, coords * 7.3 + float2(globalTime * 0.1, 0)).r;
    float warpAngle = warpNoise * 16;
    float2 warpNoiseOffset = float2(cos(warpAngle), sin(warpAngle));
    
    // Warp and pixelate coords again.
    coords += scatterDirectionBias * disintegrationFactor * (1 - warpNoise) * 0.06;
    coords = round((coords + warpNoiseOffset * disintegrationFactor * 0.01) / pixelationFactor) * pixelationFactor;
    
    float4 color = tex2D(baseTexture, coords) * sampleColor;
    float crack = saturate(tex2D(crackTexture, coords * 2.967 + identity * 0.12).r - tex2D(crackTexture, coords * 0.93 + identity * 0.75).r);
    color += crack * color.a * 1.2;
    
    return float4(ColorDodge(float3(0, 0.36, 0.99), color.rgb, 0.11), 1) * color.a * sampleColor.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}