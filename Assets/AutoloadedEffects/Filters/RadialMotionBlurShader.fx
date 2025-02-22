sampler screenTexture : register(s0);

float blurIntensity;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = 0;
    for (int i = 0; i < 20; i++)
    {
        float2 localCoords = (coords - 0.5) * (1 + i * blurIntensity * 0.01) + 0.5;
        baseColor += tex2D(screenTexture, localCoords) * 0.05;
    }
    
    return baseColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
