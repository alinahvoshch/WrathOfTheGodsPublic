sampler starTexture : register(s1);

float globalTime;
float2 screenSize;
matrix projection;

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

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, projection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Calculate various coordinates in advance.
    float2 coords = input.TextureCoordinates;
    float2 position = input.Position.xy;
    float2 screenCoords = position / screenSize;
    
    float distanceFromCenter = distance(coords, 0.5);
    float glow = 0.04 / distanceFromCenter * smoothstep(0.4, 0.1, distanceFromCenter);

    return tex2D(starTexture, coords) * input.Color + glow * input.Color.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
