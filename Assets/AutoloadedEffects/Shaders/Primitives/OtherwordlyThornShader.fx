sampler roughnessTexture : register(s1);

float cutoffY;
float localTime;
matrix uWorldViewProjection;
float2x2 rotation;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    
    input.Position.xz *= 1 - input.TextureCoordinates.y;
    input.Position.xz *= lerp(0.89, 1, sin(input.TextureCoordinates.y * 20));
    
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    float4 color = input.Color;
    
    clip(cutoffY - input.Position.y);
    
    return float4(0, 0, 0, smoothstep(0, 50, cutoffY - input.Position.y));
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
