sampler noiseTexture : register(s1);

float globalTime;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = float2(1, 0) * (atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5);
    float glow = 1 - sqrt(tex2D(noiseTexture, uv * 2.5 + globalTime * 0.1) * tex2D(noiseTexture, uv * 4 - globalTime * 0.12));
    
    float edge = 0.5 - tex2D(noiseTexture, uv + globalTime * 0.02) * 0.2;
    float opacity = smoothstep(edge, 0, distance(coords, 0.5)) * 4;
    
    return saturate(sampleColor * glow + smoothstep(1, 3, glow * sampleColor.a) * 2) * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}