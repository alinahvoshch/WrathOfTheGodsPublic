sampler baseTexture : register(s0);
sampler highlightNoiseTexture : register(s1);

float globalTime;
float highlightFocus;
float2 highlightNoiseZoom;
float4 highlightColor1;
float4 highlightColor2;
float4 backgroundColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate the base color.
    float4 color = tex2D(baseTexture, coords);
    
    // Use the distance from the center of the portrait to calculate the highlight intensity of hte pixel.
    float distanceFromCenter = distance(coords, 0.5);
    float highlightInterpolant = exp(distanceFromCenter * -highlightFocus);
    
    // Calculate a noise value that will influence the maximum highlight color.
    float highlightNoise = tex2D(highlightNoiseTexture, (coords + float2(0, globalTime * 0.1)) * highlightNoiseZoom);
    highlightNoise = tex2D(highlightNoiseTexture, (coords + float2(0, globalTime * 0.05)) * highlightNoiseZoom + highlightNoise * 0.1);
    
    // Use the noise to calculate the highlight color.
    float4 highlightColor = lerp(highlightColor1, highlightColor2, highlightNoise);
    
    // Combine the background and highlight colors based on the highlight interpolant.
    float4 specialColor = lerp(backgroundColor, highlightColor, highlightInterpolant);
    
    // Use the combined result if the base color is pure green.
    float specialColorInterpolant = color.g >= 0.999;
    float4 result = lerp(color, specialColor, specialColorInterpolant) * sampleColor;
    return result;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}