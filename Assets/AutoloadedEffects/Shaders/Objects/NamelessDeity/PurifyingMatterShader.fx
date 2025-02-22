sampler baseTexture : register(s0);
sampler pencilSketchTexture : register(s1);

float globalTime;
float gradientCount;
float maxJitter;
float2 pixelationFactor;
float3 gradient[5];

float3 PaletteLerp(float interpolant)
{
    float startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    float endIndex = clamp(startIndex + 1, 0, gradientCount - 1);
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float2 Random(float2 p)
{
    p = frac(p * float2(314.159, 314.265));
    p += dot(p, p.yx + 17.17);
    return frac((p.xx + p.yx) * p.xy);
}

float3 ColorDodge(float3 a, float3 b)
{
    return step(0, b) * lerp(min(1, b / (1 - a)), 1, step(1, a));
}

float4 CalculateImpureColor(float2 coords)
{
    coords = floor(coords / pixelationFactor) * pixelationFactor;
    
    float4 color = tex2D(baseTexture, coords);
    
    float2 randomPolar = Random(coords) * float2(1.5707, 1);
    float2 blurSampleOffset = float2(cos(randomPolar.x), sin(randomPolar.x)) * randomPolar.y;
    float4 blurredColor = tex2D(baseTexture, coords + blurSampleOffset * pixelationFactor * 2);
    
    float distanceFromCenter = distance(coords, 0.5);
    float brightness = dot(ColorDodge(color.rgb, 0.9 - blurredColor.rgb), float3(0.3, 0.6, 0.1));
    float hue = brightness * 0.75;
    float4 result = float4(PaletteLerp(hue), 1);
    float4 edge = smoothstep(0.35, 0.4, distanceFromCenter) * smoothstep(0.43, 0.4, distanceFromCenter);
    
    return result * smoothstep(0.4, 0.35, distanceFromCenter) + edge;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 impureColor = CalculateImpureColor(coords);

    return impureColor * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}