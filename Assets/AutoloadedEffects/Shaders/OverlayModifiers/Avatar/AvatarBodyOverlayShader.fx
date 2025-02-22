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

bool useBlur;
float globalTime;
float glowInterpolant;
float blurWeights[7];
float blurOffset;
float2 centerPosition;
float2 headDirection;
float2 headPosition;
float3 topLeftGlowColor;
float3 topRightGlowColor;
float3 bottomLeftGlowColor;
float3 bottomRightGlowColor;

float SignedDistanceToLine(float2 p, float2 linePoint, float2 lineDirection)
{
    return dot(lineDirection, p - linePoint);
}

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
    
    // Calculate how close the pixel is the line created by the Avatar's neck. If it's on the left side, it should be red.
    // If it's on the right side, it should be cyan.
    float distanceToHeadLine = SignedDistanceToLine(coords, headPosition, headDirection);
    float perpendicularDistance = SignedDistanceToLine(coords, centerPosition, float2(headDirection.y, headDirection.x));
    
    float3 leftGlowColor = lerp(topLeftGlowColor, bottomLeftGlowColor, smoothstep(-0.05, 0.05, perpendicularDistance));
    float3 rightGlowColor = lerp(topRightGlowColor, bottomRightGlowColor, smoothstep(-0.05, 0.05, perpendicularDistance));
    
    float rightGlowInterpolant = smoothstep(-0.045, 0.02, distanceToHeadLine);
    float3 baseBlendColor = lerp(leftGlowColor, rightGlowColor, rightGlowInterpolant);
    baseBlendColor += smoothstep(0.5, 0, color.a) * (1 - rightGlowInterpolant) * 0.6;
    
    // Calculate blur colors to the side. This will be used to give a general blur to everything.
    float4 blur = GaussianBlur(coords, sampleColor);
    
    // Calculate the blend color. It is negated on dark colors.
    float luminosity = dot(color.rgb, 0.333);
    float3 blendColor = baseBlendColor * glowInterpolant * smoothstep(0.2, 0.23, luminosity);
    blendColor += smoothstep(0.7, 0.95, luminosity) * 0.4;
    
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