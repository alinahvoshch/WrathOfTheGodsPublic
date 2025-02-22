sampler baseTexture : register(s1);

float globalTime;
float distortionIntensity;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
    float3 Normal : NORMAL0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    output.Normal = input.Normal;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates;
    float3 triangleCoords = input.Normal;
    float proximityToEdge = min(min(triangleCoords.x, triangleCoords.y), triangleCoords.z);
    float edgeDistortion = smoothstep(0.4, 0, proximityToEdge);
    float edgeGlow = smoothstep(0.1, 0, proximityToEdge);
    
    return tex2D(baseTexture, coords - edgeDistortion * distortionIntensity * 2) * color + edgeGlow * distortionIntensity * 0.95;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
