sampler freezeTexture : register(s0);
sampler accentTexture : register(s2);

float globalTime;
float glowIntensityFactor;
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

float CalculateGlowInterpolant(float distanceFromCenter, float edgeRadius, float noiseAccent)
{
    float distanceFromEdge = distance(distanceFromCenter + (noiseAccent - 0.5) * 0.16, edgeRadius);
    return saturate(pow(0.11 / distanceFromEdge, 2) * smoothstep(0.1, 0.03, distanceFromEdge) * noiseAccent);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    coords.y = (input.TextureCoordinates.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    float distanceFromCenter = distance(coords, 0.5);    
    float2 polar = float2(distanceFromCenter, atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5);
    polar.x -= globalTime * 0.5;
    polar.y += globalTime * 0.3;
    
    float opacity = input.Color.a;
    float outerRadius = 0.35;
    float cyanRadius = 0.29;
    float blueRadius = 0.23;
    float endRadius = 0.31;
    outerRadius = lerp(outerRadius, endRadius, 1 - opacity);
    cyanRadius = lerp(cyanRadius, endRadius, 1 - opacity);
    blueRadius = lerp(blueRadius, endRadius, 1 - opacity);
    
    float outerGlow = CalculateGlowInterpolant(distanceFromCenter, outerRadius, tex2D(freezeTexture, polar * 3).r);
    float cyanGlow = CalculateGlowInterpolant(distanceFromCenter, cyanRadius, tex2D(freezeTexture, polar * 2).r);
    float blueGlow = CalculateGlowInterpolant(distanceFromCenter, blueRadius, tex2D(freezeTexture, polar).r);
    float baseBrightness = clamp(outerGlow + cyanGlow + blueGlow, 0, 0.9);
    baseBrightness += baseBrightness * (tex2D(accentTexture, polar * 6 + coords) * 0.3 + tex2D(accentTexture, polar * 3 + coords * 2) * 0.3);
    
    float4 color = float4(input.Color.rgb, 1) * saturate(baseBrightness - float4(0.5, 0.27, 0.18, 0) * cyanGlow - blueGlow * float4(0.2, 0.15, 0.1, 0)) * pow(opacity, 0.3);
    
    return color * (glowIntensityFactor + 1);
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
