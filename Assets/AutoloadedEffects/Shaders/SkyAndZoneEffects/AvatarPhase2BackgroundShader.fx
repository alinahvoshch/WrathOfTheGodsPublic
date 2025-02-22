sampler baseTexture : register(s0);
sampler brightnessTexture : register(s1);
sampler windTexture : register(s2);
sampler antiBandingNoiseTexture : register(s3);
sampler darkeningAccentTexture : register(s4);

float time;
float windPrevalence;
float brightnessMaskDetail;
float brightnessNoiseVariance;
float intensity;
float gradientCount;
float arcCurvature;
float vignettePower;
float2 screenOffset;
float4 backgroundBaseColor;
float4 vignetteColor;
float4 gradient[4];

float SignedPow(float x, float n)
{
    return pow(abs(x), n) * sign(x);
}

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float4 PaletteLerp(float interpolant)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float windDirection = SignedPow(coords.y - 0.5, 0.75);
    float2 scrollOffset = float2(time * 0.2, pow(frac(coords.x) - 0.5, 2) * windDirection * -arcCurvature);
    
    float2 curvedWindCoords = ((coords + screenOffset) * 3.1 + scrollOffset) * float2(0.2, 0.6);
    
    // Calculate a moving brightness mask value that scrolls over the screen.
    // This is exceptionally important, as it effectively describes the shape of the resulting wind.
    float brightnessMaskInterpolant = (tex2D(brightnessTexture, curvedWindCoords * 1.1 + float2(time * 0.14, sin(coords.x * 8 + time * 3.1) * 0.03)) +
                                      tex2D(brightnessTexture, curvedWindCoords * 1.35 + float2(time * 0.09, sin(coords.x * 9 + time * 4.4) * 0.02))) * 0.5;
    
    // This is a SUPER important step, adding high frequency noise with a low amplitude to the brightness mask.
    // In effect, this accomplishes the effect of creating crisp detail in the wind. Without this, the entire effect loses its spark and it just looks like noise scrolling by.
    brightnessMaskInterpolant -= tex2D(windTexture, curvedWindCoords * brightnessMaskDetail + float2(time * 0.6, 0)) * 0.09;
    
    // Feed the mask interpolant through an inverse lerp function to sharpen it.
    float brightnessMask = smoothstep(0.5 - brightnessNoiseVariance * 0.5, 0.5, brightnessMaskInterpolant);
    
    // The brightness mask has been defined. That dictates the shape of wind.
    // Now, it's time to calculate the noise that dictates the hue.
    // This will naturally be multiplied by the mask at the end.
    
    float hueNoiseDetail = tex2D(brightnessTexture, curvedWindCoords * 0.8 + float2(time * 0.14, 0)) * 0.2;
    float noiseVerticalOffset = sin(coords.x * 72 + time * 9.2) * 0.013 + hueNoiseDetail;
    float hueNoiseBase = 0.6 - tex2D(windTexture, curvedWindCoords * 2 + float2(time * 0.05, noiseVerticalOffset));
    
    // Combine the hue noise with the brightness mask.
    float hue = saturate(hueNoiseBase * windPrevalence - brightnessMask) * 0.8;
    
    // Sharpen colors a bit as the brightness mask increases.
    float windColorExponent = lerp(0.9, 1.5, brightnessMask);
    float goAwayBanding = tex2D(antiBandingNoiseTexture, coords * 50) * 0.05;
    float4 localVignetteColor = vignetteColor * pow(distance(coords, 0.5) + goAwayBanding, vignettePower);
    float4 windColor = pow(PaletteLerp(hue), windColorExponent) + backgroundBaseColor + localVignetteColor;
    
    float2 offsetFromTop = coords - float2(0.5, -0.3);
    float2 polar = float2(atan2(offsetFromTop.y, offsetFromTop.x) / 6.283 + 0.5, length(offsetFromTop));
    float darkeningNoise = tex2D(darkeningAccentTexture, polar * float2(3, 3.3) + time * float2(0.1, -0.04) + brightnessMaskInterpolant * 0.75);
    float darkening = smoothstep(0.25, 0, hue) * darkeningNoise;
    windColor.rgb *= lerp(1.5, 0.1, darkening);
    
    return windColor * intensity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}