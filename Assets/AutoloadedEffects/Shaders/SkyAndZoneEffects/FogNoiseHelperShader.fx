sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float endY;
float2 screenUV;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords.y = smoothstep(0, endY, coords.y);
    
    float noise = tex2D(noiseTexture, coords * 9 + screenUV + float2(globalTime * 0.005, 0)) * coords.y * 0.25;
    float4 baseColor = tex2D(baseTexture, coords + float2(0, noise)) * sampleColor;
    float4 color = baseColor * sqrt(baseColor.a) * 2.4;
    
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}