texture baseTexture;
sampler2D baseTextureSampler = sampler_state
{
    texture = <baseTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float globalTime;
float glowInterpolant;
float blurWeights[7];
float blurOffset;
float3 glowColor;

// Corresponds to Clip Studio Paint's Add (Glow) blending function.
float AddGlowBlend(float a, float b)
{
    return min(1, a + b);
}

float3 AddGlowBlend(float3 a, float3 b)
{
    return float3(AddGlowBlend(a.r, b.r), AddGlowBlend(a.g, b.g), AddGlowBlend(a.b, b.b));
}

float4 GaussianBlur(float2 coords, float4 sampleColor)
{
    float4 result = 0;
    for (int i = -3; i < 4; i++)
    {
        for (int j = -3; j < 4; j++)
        {
            float weight = blurWeights[abs(i) + abs(j)];
            result += tex2D(baseTextureSampler, coords + float2(i, j) * blurOffset) * weight;
        }
    }
    
    return result * sampleColor;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(baseTextureSampler, coords) * sampleColor;
    
    float3 baseBlendColor = glowColor + smoothstep(0.5, 0, color.a) * 0.6;
    
    // Calculate blur colors to the side. This will be used to give a general blur to everything.
    float4 blur = GaussianBlur(coords, sampleColor);
    
    // Calculate the blend color. It is negated on dark colors.
    float luminosity = dot(color.rgb, 0.333);
    float3 blendColor = baseBlendColor * glowInterpolant * smoothstep(0.2, 0.23, luminosity);
    
    // Blend colors together.
    float4 blendGlow = float4(blur.rgb * baseBlendColor, 0);
    
    return float4(AddGlowBlend(color.rgb, blendColor), 1) * smoothstep(0, 0.3, color.a) + blendGlow;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}