sampler baseTexture : register(s0);
sampler distanceFieldTexture : register(s1);

float globalTime;

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Calculate how far the current pixel is from the edge of the distance field.
    float distanceFromCenter = distance(coords, 0.5);
    float edgeDistance = tex2D(distanceFieldTexture, coords) * 0.03;
    float distanceFromEdge = max(0.0001, distanceFromCenter - edgeDistance);
    float glow = 0.02 / distanceFromEdge * smoothstep(0.08, 0, distanceFromEdge);
    
    return clamp(glow, 0, 1.7) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}