sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float4 innerGlowColor;

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distance(coords, 0.5));
    float distanceFromEdge = 0.5 - polar.y;
    
    float angleOffsetNoise = tex2D(noiseTexture, polar * float2(1, 3) + float2(globalTime * 0.2, 0));
    float angle = angleOffsetNoise * pow(polar.y, 1.8) * 12 + pow(polar.y, 0.4) * 30 - globalTime * 10;
    
    coords = RotatedBy(coords - 0.5, angle) + 0.5;
    
    float innerGlow = smoothstep(0.25, 0.01, polar.y) * 2;
    return (tex2D(baseTexture, coords) * sampleColor + innerGlow * innerGlowColor) * distanceFromEdge * 3;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}