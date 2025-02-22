sampler baseTexture : register(s0);
sampler staticTexture : register(s1);

float globalTime;
float neutralizationInterpolant;
float staticInterpolant;
float staticZoomFactor;
float scrollTimeFactor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float staticOverlay = tex2D(staticTexture, coords * (staticZoomFactor + 1) * 0.5 + globalTime * (scrollTimeFactor + 1) * 21.494754);
    staticOverlay = sqrt(staticOverlay * tex2D(staticTexture, coords * (staticZoomFactor + 1) * 0.55 + globalTime * (scrollTimeFactor + 1) * 27.1));
    
    float4 color = tex2D(baseTexture, coords);
    float staticAdditive = pow(staticOverlay, 1.5) * color.a;
    float4 staticColor = color + staticAdditive * 0.45;
    float4 result = lerp(staticColor, float4(staticAdditive, staticAdditive, staticAdditive, 1), staticInterpolant);
    
    return lerp(result, float4(0.5, 0.5, 0.5, 1), neutralizationInterpolant) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}