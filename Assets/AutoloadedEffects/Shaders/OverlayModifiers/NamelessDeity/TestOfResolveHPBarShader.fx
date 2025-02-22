sampler baseTexture : register(s0);
sampler starTexture : register(s1);

float time;
float2 imageSize;
float4 sourceRectangle;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position :SV_Position) : COLOR0
{
    return float4(0, 0, 0, 1);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}