sampler baseTexture : register(s0);
sampler latticeTexture : register(s1);

float globalTime;
float2 imageSize;
float4 sourceRectangle;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 framedCoords = (coords * imageSize - sourceRectangle.xy) / sourceRectangle.zw;
    float4 color = tex2D(baseTexture, coords).a * sampleColor;
    float4 latticeColor = 1 - tex2D(latticeTexture, position.xy / 200);
    
    return color + latticeColor * 0.785 + distance(framedCoords.y, 0.5) * 1.1;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}