sampler bloodBlobTexture : register(s1);

float localTime;
float3 edgeColor;
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
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    output.TextureCoordinates.y = (output.TextureCoordinates.y - 0.5) / input.TextureCoordinates.z + 0.5;

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
    
    // Calculate the blob texture noise value.
    float blobTextureNoise = 1 - tex2D(bloodBlobTexture, (coords + float2(localTime * -1.2, 0)) * float2(1.75, 1) * 0.24);
    blobTextureNoise -= tex2D(bloodBlobTexture, (coords + float2(localTime * -1.1, 0)) * float2(1.57, 1) * 0.3) * 0.16;
    
    // Calculate whether this pixel should be erased.
    // This varies based on how far along the prim this pixel is, and how far it is from the horizontal center.
    // Erasure effects are nullified at the tip of the prim.
    float erasureThreshold = pow(coords.x, 1.4) * 0.9 + smoothstep(0.8, 1, coords.x) * 1.3 + (1 - QuadraticBump(coords.y)) * coords.x * 0.7;
    erasureThreshold = lerp(erasureThreshold, 0, smoothstep(0.35, 0.22, coords.x));
    bool erasePixel = blobTextureNoise <= erasureThreshold;
    
    float colorBias = smoothstep(2, 1.5, blobTextureNoise / erasureThreshold) + smoothstep(0.3, 0.18, QuadraticBump(coords.y));
    color -= float4(1 - edgeColor, 0) * colorBias;
    
    return color * (1 - erasePixel);
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
