sampler baseTexture : register(s0);

float globalTime;
float2 scrollIncrement;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords += scrollIncrement * globalTime;
    return tex2D(baseTexture, frac(coords)) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}