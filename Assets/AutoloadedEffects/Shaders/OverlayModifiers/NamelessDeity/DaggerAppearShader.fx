sampler baseTexture : register(s0);
sampler iridescenceTexture : register(s1);

float warpOffset;
bool justWarp;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float pi = 3.141;
    float verticalOffset = sin(coords.x * pi * 3 + warpOffset * 6.283) * warpOffset * coords.x * 0.2;
    float2 warp = float2(0, verticalOffset);
    float4 colorData = tex2D(baseTexture, coords + warp);
    float fadeFromWhite = saturate(colorData.r - colorData.g - colorData.b);
    
    float4 iridescence = tex2D(iridescenceTexture, position.xy / 540) * float4(0.4, 0.4, 0.4, 1);
    float4 color = lerp(colorData.a * dot(colorData.rgb, 0.333), iridescence, fadeFromWhite);
    
    return lerp(color, colorData, justWarp) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}