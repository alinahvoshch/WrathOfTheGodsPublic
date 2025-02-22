sampler baseTexture : register(s1);

float localTime;
float4 generalColor;
float4 glowColor;
matrix uWorldViewProjection;

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
    
    float cosAngle = coords.x;
    coords.x = acos(cosAngle) / 3.141;
    
    float verticalEdgeGlow = smoothstep(0.25, 0, coords.y) + smoothstep(0.75, 1, coords.y);
    float spin = localTime * sign(coords.y) * 0.2;
    
    coords.y = 1 - coords.y;
    
    return tex2D(baseTexture, coords + float2(spin, 0)) * generalColor + verticalEdgeGlow * glowColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
