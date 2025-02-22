sampler baseTexture : register(s0);

float blurOffset;
float blurWeights[9];

float4 GaussianBlur(float2 coords)
{
    float4 result = 0;
    for (int i = -2; i < 3; i++)
    {
        for (int j = -2; j < 3; j++)
        {
            float weight = blurWeights[abs(i) + abs(j)];
            result += tex2D(baseTexture, coords + float2(i, j) * blurOffset).a * weight;
        }
    }
    
    return result;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = GaussianBlur(coords);
    float brightness = dot(color.rgb, float3(0.3, 0.6, 0.1));
    
    return float4(brightness, brightness, brightness, 1) * sampleColor * color.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}