sampler obeliskTexture : register(s1);

float globalTime;
float3 lightDirection;
float2 viewPosition;

matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
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
    // Calculate various coordinates in advance.
    float2 coords = input.TextureCoordinates;
    float2 position = input.Position.xy;
    
    float4 color = input.Color;
    float brightness = pow(saturate(dot(input.Normal, lightDirection)), 2);
    color.rgb *= brightness;
    
    float3 viewDirection = normalize(float3(viewPosition - position, 0));
    float3 reflectDirection = reflect(input.Normal, -lightDirection);
    float specular = pow(saturate(dot(viewDirection, reflectDirection)), 16);
    color.rgb *= 1 + specular * 9;
    
    return tex2D(obeliskTexture, coords) * color;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
