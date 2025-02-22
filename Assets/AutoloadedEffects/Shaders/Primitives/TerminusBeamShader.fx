sampler streakTexture : register(s1);

float globalTime;
float bulgeVerticalOffset;
float bulgeVerticalReach;
float redEdgeThreshold;
float bulgeHorizontalReach;
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

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

float Cos01(float x)
{
    return cos(x) * 0.5 + 0.5;
}

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    
    float x = input.TextureCoordinates.x;
    float y = input.TextureCoordinates.y;
    y = (y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    float spikeInterpolant = smoothstep(0, bulgeVerticalReach, x - bulgeVerticalOffset);
    float spike = Cos01(spikeInterpolant * 12.566 - globalTime * 8 + y * 200) + Cos01(spikeInterpolant * 37.699 - globalTime * 24) * 0.5;
    spike *= 1 - spikeInterpolant;
    spike *= smoothstep(0, 0.04, x);
    
    input.Position.x += (y - 0.5) * QuadraticBump(spikeInterpolant) * bulgeHorizontalReach;
    input.Position.x += (y - 0.5) * spike * bulgeHorizontalReach * 0.59;
    
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates;
    
    // Account for texture distortion artifacts in accordance with the primitive distortion fixes.
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    float edgeInterpolant = 1 - QuadraticBump(coords.y);
    
    return edgeInterpolant >= redEdgeThreshold ? color : color.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
