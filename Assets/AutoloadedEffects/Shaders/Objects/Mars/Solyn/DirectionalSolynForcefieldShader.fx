sampler baseTexture : register(s0);
sampler distanceFieldTexture : register(s1);

float globalTime;
float glowIntensity;
float4 colorA;
float4 colorB;

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Bend the texture backwards a bit, to help make the forcefield feel arched.
    float warpAngle = (coords.x - 0.5) * -6.283 + globalTime * 2;
    float2 warpedCoords = RotatedBy(coords - 0.5, warpAngle) + 0.5;
    
    // Calculate how far the current pixel is from the edge of the distance field.
    float distanceFromCenter = distance(coords, 0.5);
    float edgeDistance = tex2D(distanceFieldTexture, warpedCoords) * 0.05;
    float distanceFromEdge = max(0.0001, distanceFromCenter - edgeDistance);
    
    // Use the distance value to calculate how much the current pixel should glow.
    float glow = 0.01 / distanceFromEdge * smoothstep(0.08, 0, distanceFromEdge) * glowIntensity;
    
    // Calculate the general color.
    float colorInterpolant = cos(distanceFromEdge * 120 - globalTime * 10 - length(position.xy) * 0.02) * 0.5 + 0.5;
    float4 generalColor = lerp(colorA, colorB, colorInterpolant);
    
    float4 color = saturate(tex2D(baseTexture, coords) * glow * generalColor);
    return color * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}