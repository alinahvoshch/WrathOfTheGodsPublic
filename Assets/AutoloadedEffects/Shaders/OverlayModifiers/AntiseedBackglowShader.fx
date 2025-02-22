sampler baseTexture : register(s0);

float blurWeights[9];
float blurOffset;
float fadeToWhiteInterpolant;

float4 GaussianBlur(float2 coords, float offsetFactor)
{
    float4 result = 0;
    float4 baseColor = tex2D(baseTexture, coords);
    float baseColorBlackness = smoothstep(0.1, 0.01, dot(baseColor.rgb, 0.333));
    for (int i = -4; i < 5; i++)
    {
        for (int j = -4; j < 5; j++)
        {
            float weight = blurWeights[abs(i) + abs(j)];
            
            float4 blurredColor = tex2D(baseTexture, coords + float2(i, j) * blurOffset * offsetFactor);
            float blurredColorBlackness = smoothstep(0.1, 0.01, dot(blurredColor.rgb, 0.333));

            float4 color = lerp(baseColor, blurredColor, baseColorBlackness * blurredColorBlackness);
            color = lerp(color, color.a, fadeToWhiteInterpolant);
            
            result += color * weight;
        }
    }
    
    return result;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    return GaussianBlur(coords, 0.2) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}