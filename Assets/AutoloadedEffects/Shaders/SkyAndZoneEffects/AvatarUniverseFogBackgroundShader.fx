sampler baseTexture : register(s0);
sampler fogTexture : register(s1);

float globalTime;
float arcCurvature;
float4 fogColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float windDirection = coords.y - 0.5;
    float2 scrollOffset = float2(globalTime * 0.16, pow(frac(coords.x) - 0.5, 2) * windDirection * -arcCurvature);
    
    float2 curvedWindCoords = (coords * 2.1 + scrollOffset) * float2(0.3, 0.75);
    float fogDensity = pow(tex2D(fogTexture, curvedWindCoords) + (tex2D(fogTexture, curvedWindCoords + scrollOffset * 0.6) - 0.5) * 0.9, 3);
    
    return sampleColor + fogDensity * fogColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
