sampler screenTexture : register(s0);
sampler ignoreTexture : register(s1);

float greyscaleInterpolant;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(screenTexture, coords);
    float greyscale = dot(color.rgb, float3(0.3, 0.6, 0.1));    
    float ignoreBrightness = dot(tex2D(ignoreTexture, coords).rgb, float3(0.3, 0.6, 0.1));
    float fuckYou = greyscaleInterpolant * smoothstep(0.2, 0, ignoreBrightness);
    
    return lerp(color, greyscale, fuckYou);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
