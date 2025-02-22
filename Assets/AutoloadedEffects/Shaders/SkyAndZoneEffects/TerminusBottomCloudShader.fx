sampler baseTexture : register(s0);
sampler cloudNoiseTexture : register(s1);

float globalTime;
float2 screenOffset;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords += screenOffset;
    
    float coordsOffsetNoise = sqrt(tex2D(cloudNoiseTexture, coords * 5 + float2(0.15, globalTime * 0.11)) * tex2D(cloudNoiseTexture, coords * 4 + globalTime * float2(0.03, 0.2)));
    coords += (tex2D(cloudNoiseTexture, coords * 2.3 + coordsOffsetNoise * 0.15) - 0.5) * 0.1;
    
    float opacity = smoothstep(0.7, 1, coords.y - screenOffset.y);
    float4 color = tex2D(baseTexture, coords) * sampleColor;
    
    return color * opacity;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}