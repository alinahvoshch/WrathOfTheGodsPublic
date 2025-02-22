sampler paperTexture : register(s0);
sampler noiseTexture : register(s1);

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float noise = tex2D(noiseTexture, coords * 1.67);
    float4 color = tex2D(paperTexture, coords);
    color = lerp(color, 1, noise * 0.8);
    color = lerp(color, 1, 0.4);
    
    float antialiasOpacity = 
        smoothstep(0, 0.005, coords.x) * smoothstep(1, 0.995, coords.x) *
        smoothstep(0, 0.009, coords.y) * smoothstep(1, 0.991, coords.y);
    
    return color * sampleColor * antialiasOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}