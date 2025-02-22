sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);
sampler uvOffsetMap : register(s2);
sampler uvOffsetMapBlur : register(s3);

float globalTime;
float gradientCount;
float scrollTime;
float fadeToFastScrollInterpolant;
float3 gradient[8];

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(frac(interpolant) * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float3 uvOffsetData = saturate(tex2D(uvOffsetMap, coords).rga + tex2D(uvOffsetMapBlur, coords).rga * float3(0, 0, 1));
    uvOffsetData.z = pow(uvOffsetData.z, 0.61);
    
    float scroll = uvOffsetData.x + scrollTime;
    float noise = tex2D(noiseTexture, float2(scroll, scroll * 0.2)) * 2.6;
    
    float4 baseColor = tex2D(baseTexture, coords) * sampleColor;
    float4 gradientColor = saturate(float4(PaletteLerp(noise), 1) + (1 - uvOffsetData.y) * uvOffsetData.z * 0.7);
    
    return lerp(baseColor, gradientColor, uvOffsetData.z * fadeToFastScrollInterpolant);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}