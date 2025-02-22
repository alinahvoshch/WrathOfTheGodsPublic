sampler glowNoiseTexture : register(s1);
sampler darkenNoiseTexture : register(s2);

float globalTime;
float edgeGlowIntensity;
float centerGlowExponent;
float centerGlowCoefficient;
float centerDarkeningFactor;
float innerScrollSpeed;
float middleScrollSpeed;
float outerScrollSpeed;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

float CalculateGlowInfluence(float2 coords, float scrollTime, float glowCenter, float distanceRange)
{
    float glowNoiseA = tex2D(glowNoiseTexture, coords * float2(1.5, 0.4) + float2(-2.2, 0) * scrollTime);
    float glowNoiseB = tex2D(darkenNoiseTexture, coords * float2(0.3, 1.5) + float2(-2.5, 0) * scrollTime) * 0.5;
    float glowNoise = sqrt(glowNoiseA * glowNoiseB);
    
    float distanceFromCenter = distance(coords.y, 0.5) * 2;
    float distanceFromGlowCenter = distance(distanceFromCenter, glowCenter);
    
    return glowNoise * smoothstep(1, 0.6, distanceFromGlowCenter / distanceRange) * smoothstep(glowCenter - 0.09, glowCenter, distanceFromCenter);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    float4 color = input.Color;
    
    // Account for texture distortion artifacts in accordance with the primitive distortion fixes.
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    // Determine horizontal distance values, relative to both the midpoint and edge of the beam.
    float distanceFromCenter = distance(coords.y, 0.5);
    float distanceFromEdge = distance(distanceFromCenter, 0.4);
    
    // Use noise to darken blotches of the base color.
    color.rgb -= tex2D(darkenNoiseTexture, coords * float2(0.9, 0.5) + float2(globalTime * -1.05, 0)) * 0.85;
    
    // Calculate a glow value for the edges of the laserbeam, biasing the colors to a blazing white.
    float edgeGlow = saturate(edgeGlowIntensity / distanceFromEdge) * smoothstep(0.1, 0.05, distanceFromEdge) * color.a;
    float edgeFadeout = smoothstep(0.45, 0.4, distanceFromCenter);
    
    // Calculate a set of multiple glow noise values, each with different zones of the beam to account for, and each with differing scroll speeds.
    // Scroll speeds are faster near the center and slower at the edges due to a stronger concentration of energy that isn't dispersed at the center.
    float glowNoiseA = CalculateGlowInfluence(coords, globalTime * innerScrollSpeed, 0, 0.4);
    float glowNoiseB = CalculateGlowInfluence(coords, globalTime * middleScrollSpeed, 0.4, 0.4);
    float glowNoiseC = CalculateGlowInfluence(coords, globalTime * outerScrollSpeed, 0.8, 0.2);
    float glowNoise = glowNoiseA + glowNoiseB + glowNoiseC;
    
    // Combine the glow values.
    float4 result = clamp(edgeGlow + saturate(color + pow(glowNoise, centerGlowExponent) * centerGlowCoefficient) * edgeFadeout, 0, 200);
    
    // Make colors darker in the center, to provide depth relative to the bright edges.
    // This only affects colors that are already red, leaving glowy colors from the noise scroll alone.
    float redInterpolant = saturate(result.r - result.g - result.b);
    result.rgb -= smoothstep(0.25, 0.05, distanceFromCenter) * redInterpolant * centerDarkeningFactor;
    
    // Brighten the results dramatically at the start.
    result += smoothstep(0.15, 0, coords.x) * pow(result.a, 2) * 2;
    
    return result * color.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
