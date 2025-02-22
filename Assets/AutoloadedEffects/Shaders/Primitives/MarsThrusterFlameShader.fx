sampler fireTexture : register(s1);

float localTime;
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

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates;    
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    float noise = tex2D(fireTexture, coords + float2(-localTime * 3, 0)) + tex2D(fireTexture, coords * float2(0.3, 1) + float2(localTime * -2, 0)) * 1.2;
    color *= noise * 3;
    
    float endFadeCoarse = tex2D(fireTexture, coords * float2(0.02, 0.15) + float2(localTime * -1.96, 0)) * 0.4;
    float endFadeDetailed = tex2D(fireTexture, coords * float2(0.1, 0.5) + float2(localTime * -2.3, 0)) * 0.3;
    float fadeOutValue = coords.x + endFadeDetailed + endFadeCoarse + distance(coords.y, 0.5) * 1.5;
    float opacity = smoothstep(1, 0.85, fadeOutValue);
    
    // Make the top of the thrusters bright, regardless of texture cutouts.
    color += smoothstep(0.3, 0.05, coords.x);
    
    return saturate(color) * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
