sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distance(coords, 0.5));
    
    float distanceFromCenter = distance(coords, 0.5) + tex2D(noiseTexture, polar * float2(2, 1) + float2(0.2, 0.35) * globalTime) * polar.y * 0.2;
    float distanceFromEdge = distance(distanceFromCenter, 0.45);
    
    float4 color = tex2D(baseTexture, coords) * float4(0, 0, 0, 1);
    
    float edgeGlow = smoothstep(0.35, 0.5, distanceFromCenter) / distanceFromEdge * 0.025;
    float edgeFadeout = smoothstep(0.49, 0.46, distanceFromCenter);
    
    return saturate(color + edgeGlow * sampleColor) * edgeFadeout;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}