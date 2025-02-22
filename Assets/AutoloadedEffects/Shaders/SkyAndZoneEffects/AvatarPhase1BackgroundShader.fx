sampler baseTexture : register(s1);
sampler darkeningAccentTexture : register(s2);
sampler lightningCrackTexture : register(s3);

float globalTime;
float gradientCount;
float3 gradient[4];
float lightningFlashLifetimeRatios[15];
float2 lightningFlashPositions[15];
float lightningFlashIntensities[15];

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(frac(interpolant) * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float3 CalculateLightningColor(float4 baseColor, float2 coords)
{
    float flashIntensity = 0;
    float distanceDistortion = tex2D(baseTexture, coords * 3) - 0.5;
    
    for (int i = 0; i < 15; i++)
    {
        float lifetimeRatio = lightningFlashLifetimeRatios[i];
        
        // Calculate the offset from the flash, biasing the X coordinates based on time, to make it look like the effect is reaching downward over time.
        float squish = smoothstep(0, 0.25, lifetimeRatio);
        float2 offsetFromFlash = lightningFlashPositions[i] - coords;
        offsetFromFlash.x *= lerp(1, 5, squish);
        
        // Calculate the distance from the flash center, with the aforementioned squished offset and a distance distortion that ensures the effect doesn't look perfectly circular.
        float localDistanceFromFlash = length(offsetFromFlash) - distanceDistortion * lifetimeRatio * 0.4;
        
        // Calculate the intensity of the flash, sharply rising and then slowly dissipating.
        float localFlashIntensity = smoothstep(0, 0.1, lifetimeRatio) * smoothstep(1, 0.1, lifetimeRatio);
        
        // Calculate the brightness of lightning based on crack noise.
        float2 lightningCoords = coords * float2(4.5, 2) + lightningFlashPositions[i] * 1000 + distanceDistortion * 0.1;
        float lightningNoise = smoothstep(-0.1, 1 - lifetimeRatio * 0.5, tex2D(lightningCrackTexture, lightningCoords)) + 0.01;
        float lightningIntensity = 0.7 / lightningNoise * (1 - lifetimeRatio);
        
        // Make the intensity of lightning reach a hard cutoff based on distance from the flash source.
        lightningIntensity *= smoothstep(0.7, 0.25, localDistanceFromFlash / lightningFlashIntensities[i]);
        
        // Increment brightness.
        flashIntensity += localFlashIntensity / max(0.001, pow(localDistanceFromFlash, 1.2)) * lightningIntensity;
    }
    
    // Apply the flash intensity based on how bright the original pixel was.
    // Darker pixels are affected less to ensure that depth is preserved.
    float originalBrightness = dot(baseColor.rgb, float3(0.3, 0.6, 0.1));    
    float red = lerp(0.7, 0.1, tex2D(baseTexture, coords * 1.1));
    float interpolateFactor = smoothstep(0.05, 0.65, originalBrightness) * 0.25;
    return lerp(baseColor.rgb, float3(red, 0.5, 1), max(flashIntensity, 0) * interpolateFactor);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // WE LOVE NOISE!
    float noiseA = tex2D(baseTexture, coords * float2(0.9, 2.15) + float2(globalTime * 0.05, 0));
    float noiseAngle = noiseA * 16;
    float2 noiseDirection = float2(cos(noiseAngle), sin(noiseAngle)) * 0.03;
    float noiseB = (pow(tex2D(baseTexture, coords * float2(1.2, 7.2) - noiseDirection), 1.5) + noiseA) * 0.5;
    noiseB = pow(noiseB, 1.4) * 1.8;
    
    // Use the aforementioned multi-sampled noise values to calculate a value for gradient mapping.
    float skyColorInterpolant = clamp(noiseB * 1.1, 0, 0.8) * 0.9;
    
    // Calculate gradient mapping colors and return the result.
    float4 result = float4(PaletteLerp(skyColorInterpolant), 1) * sampleColor;
    
    float2 offsetFromTop = coords - float2(0.5, 0);
    float2 polar = float2(atan2(offsetFromTop.y, offsetFromTop.x) / 6.283 + 0.5, length(offsetFromTop));
    float darkeningNoise = tex2D(darkeningAccentTexture, polar * float2(3, 5) + globalTime * float2(0.1, -0.04) + noiseA * 0.5);
    float darkening = smoothstep(0.25, 0, skyColorInterpolant) * darkeningNoise + smoothstep(0.2, 0.7, coords.y);
    result.rgb = CalculateLightningColor(result, coords);
    result.rgb *= lerp(1, 0.1, darkening);
    
    return result;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
