sampler baseTexture : register(s1);

float2 size;
float4x4 uWorldViewProjection;

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
    
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    
    // Use an eigenvalue of the jacobian matrix to determine the amount of scrunching for the given pixel.
    float2x2 jacobian = float2x2(ddx(coords.x), ddy(coords.x), ddx(coords.y), ddy(coords.y));
    float a = 1;
    float b = jacobian._11 + jacobian._22;
    float c = determinant(jacobian);
    float lambda = (-b + sqrt(b * b - a * c * 4)) / (a * 2);
    
    // Use the aforementioned eigenvalue to make more scrunched pixels darker overall.
    float darkeningInterpolant = pow(smoothstep(0, 0.01, -lambda), 2);
    float3 darkeningColor = lerp(1, 0.8, darkeningInterpolant) * 1.2;
    
    // Combine everything together.
    float2 pixelatedCoords = round(coords * 75) / 75;
    float2 flagCoords = (pixelatedCoords * float2(size.x / size.y, 1) - 0.5) * 1.12 + 0.5;
    
    return tex2D(baseTexture, flagCoords) * input.Color * float4(darkeningColor, 1);
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
