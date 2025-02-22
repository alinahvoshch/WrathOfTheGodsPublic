sampler kalisetFractal : register(s1);
sampler noiseTexture : register(s2);
sampler rainbowTexture : register(s3);

float time;
float zoom;
float scrollSpeedFactor;
float brightness;
float detailIterations;
float3 frontStarColor;
float3 backStarColor;
float4 vignetteColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 result = 0;
    float volumetricLayerFade = 1.0;
    float distanceFromBottom = distance(coords.y, 1);
    float detailIterationsClamped = clamp(detailIterations, 1, 20);
    float2 center = float2(0.22, 0.1);
    
    for (int i = 0; i < detailIterationsClamped; i++)
    {
        float scrollTime = time * pow(volumetricLayerFade, 1.6) * 2.1;
        float2 p = (coords - center) * zoom / volumetricLayerFade + center;

        // Perform scrolling behaviors. Each layer should scroll a bit slower than the previous one, to give an illusion of 3D.
        p += scrollSpeedFactor * scrollTime * float2(1, 1 - i * 0.0719);

        float totalChange = tex2D(kalisetFractal, p);
        float3 localFrontStarColor = frontStarColor + float3(tex2D(noiseTexture, p * 15.85).r * 0.25, 0, 0);
        float3 localBackStarColor = backStarColor + float3(0, 0, tex2D(noiseTexture, p * 31).r * 0.5);
        
        float4 layerColor = float4(lerp(localFrontStarColor, localBackStarColor, i / detailIterationsClamped), 1.0);
        result += layerColor * totalChange * volumetricLayerFade;

        // Make the next layer exponentially weaker in intensity.
        volumetricLayerFade *= 0.875;
    }
    
    // Apply color change interpolants. This will be used later.
    float colorChangeBrightness1 = tex2D(noiseTexture, coords * 2.51 - time * 0.025);
    float colorChangeBrightness2 = tex2D(noiseTexture, coords * 3.65 + time * 0.042);
    float totalColorChange = colorChangeBrightness1 + colorChangeBrightness2;

    // Account for the accumulated scale from the fractal noise.
    float warpFactor = smoothstep(1000, 400, result.r) * 0.9;
    float brightnessNoise = (tex2D(noiseTexture, coords * 10.1 + result.r * warpFactor * 0.003) + tex2D(noiseTexture, coords * 5.3 + result.b * warpFactor * 0.0026)) * 0.5;
    float brightnessExponent = lerp(0.04, 1.05, brightnessNoise);
    result.rgb = 1 - exp(pow(result.rgb * 0.012714, 2.64 - totalColorChange * 2.4 + pow(distanceFromBottom, 3) * 3.9) * brightness * -brightnessExponent);
    
    // Add a bit of rainbow to brighter patches to make them feel a bit more ethereal.
    float brightness = dot(result.rgb, float3(0.3, 0.6, 0.1));
    float3 rainbow = tex2D(rainbowTexture, coords * 6.9 + result.b * 0.09);
    result.rgb += rainbow * smoothstep(0.32, 0.67, brightness) * 0.6;
    
    // Apply a vignette.
    float distanceFromCenter = distance(coords, center);
    result += smoothstep(0.15, 0.24, distanceFromCenter) * lerp(0.6, 0.75, result.r) * vignetteColor * brightness;
    
    return result * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
