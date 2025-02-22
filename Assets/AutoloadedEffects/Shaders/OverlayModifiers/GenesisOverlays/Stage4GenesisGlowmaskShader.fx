sampler baseTexture : register(s0);
sampler glowmaskTexture : register(s1);
sampler timeScrollTexture : register(s2);
sampler gradientTexture : register(s3);

float globalTime;
float glowMotionInterpolant;

float3 ColorDodge(float3 a, float3 b)
{
    return a / (1 - b);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float timeScrollData = tex2D(timeScrollTexture, coords).r;
    float3 gradient = tex2D(gradientTexture, float2(timeScrollData * 2, 0.5));
    
    float4 baseColor = tex2D(baseTexture, coords);
    float3 glowmaskColor = sampleColor.rgb;
    float3 color = ColorDodge(baseColor.rgb, tex2D(glowmaskTexture, coords).rgb * glowmaskColor);
    
    color = lerp(color, gradient, smoothstep(0, 0.5, timeScrollData) * glowMotionInterpolant);
    
    return float4(color, 1) * baseColor.a * sampleColor.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}