sampler baseTexture : register(s0);
sampler noiseTextureA : register(s1);
sampler backgroundTexture : register(s2);
sampler noiseTextureB : register(s3);

float globalTime;
float detail;
float gradientCount;
float starDispersionExponent;
float twinkleRate;
float galaxyBrightnessFactor;
float galaxyScale;
float edgeSpiralArmFadeExponent;
float twinkleBrightness;
float3 gradient[6];

// Original parameter definitions:
/*
    Vector3[] palette = new Vector3[]
    {
        new Vector3(0.2f, 0.5f, 0.73f),
        new Vector3(0.51f, 0.3f, 0.78f),
        new Vector3(0.4f, 0.71f, 0.9f),
    };

    ManagedShader galaxyShader = ShaderManager.GetShader("NoxusBoss.GalaxyBackgroundShader");
    galaxyShader.SetTexture(WavyBlotchNoiseDetailed, 1, SamplerState.LinearWrap);
    galaxyShader.SetTexture(PerlinNoise, 2, SamplerState.LinearWrap);
    galaxyShader.TrySetParameter("gradient", palette);
    galaxyShader.TrySetParameter("gradientCount", palette.Length);
    galaxyShader.TrySetParameter("starDispersionExponent", 9.3f);
    galaxyShader.TrySetParameter("twinkleRate", 1.1f);
    galaxyShader.TrySetParameter("galaxyBrightnessFactor", 3f);
    galaxyShader.TrySetParameter("galaxyScale", 0.67f);
    galaxyShader.TrySetParameter("edgeSpiralArmFadeExponent", 13.1f);
    galaxyShader.TrySetParameter("twinkleBrightness", 0.93f);
    galaxyShader.Apply();
*/

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = clamp(startIndex + 1, 0, gradientCount - 1);
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float Noise(float2 coords)
{
    float2 warp = tex2D(noiseTextureA, coords * 3.3 + float2(globalTime * 0.1, 0)) * 0.08;
    float secondaryNoise = lerp(tex2D(noiseTextureA, coords * 2 - warp), tex2D(noiseTextureB, coords * 2 - warp), detail);
    
    return tex2D(noiseTextureA, coords + warp) + secondaryNoise;
}

float Noise3(float3 coords)
{
    float a = Noise(coords.xy);
    float b = Noise(coords.xy + 0.43);
    float fadeInterpolant = cos(coords.z * 6.283 + coords.x * 20 - coords.y * 20) * 0.5 + 0.5;
    
    return lerp(a, b, fadeInterpolant);
}

float InverseLerp(float a, float b, float t)
{
    return saturate((t - a) / (b - a));
}

float CalculateStarBrightness(float2 coords, float cutoffThreshold)
{
    // Generate a random value from the input coordinates, and determine whether it can twinkle based on whether it exceeds a given inputted threshold.
    float randomValue = frac(sin(dot(coords + cutoffThreshold, float2(12.9898, 78.233))) * 97000);
    bool createTwinkle = randomValue >= cutoffThreshold;
    
    // Assuming it does exceed said threshold, SIGNIFICANTLY squash down the spectrum of values that can result in a twinkle, first applying an inverse lerp starting
    // at the threshold, and then applying a harsh exponentiation.
    // InverseLerp is used here because it has a slightly nicer look than smoothstep does.
    float brightness = pow(InverseLerp(cutoffThreshold, 1, randomValue), starDispersionExponent);
    
    // Multiply the brightness value by a shining twinkle factor, to make it give the twinkly look to the stars.
    // This varies strongly based on brightness and the cutoff threshold to ensure random twinkling timers for each star.
    float twinkle = cos(globalTime * twinkleRate * 6.283 + randomValue * 400 + cutoffThreshold * 400) * 0.5 + 0.5;
    brightness *= twinkle;
    
    return brightness * createTwinkle;
}

float4 CalculateGalaxyColor(float2 coords, float scaleFactor)
{
    // Volumetrically step through 3D space, accumulating density values along the way.
    float totalDensity = 0;
    float scale = galaxyScale * scaleFactor;
    float spinTime = globalTime * 0.06;
    for (float i = 0; i < 10; i++)
    {
        float z = i / 10;        
        float3 coords3D = float3((coords - 0.5) / scale + 0.5, z);
        float distanceFromCenter = distance(coords3D, 0.5);
                
        // Combine two twinkling, low resolution star values together to determine the effects of twinkling stars on the galaxy.
        float twinkle = CalculateStarBrightness(coords, 0.5) + CalculateStarBrightness(coords * 2.5, 0.99);
        
        // Calculate the base density for this step based on coords swirled around based distance from the center of the scene and time, to create the spiral arms of the galaxy.
        float distanceFade = smoothstep(0.1, 1, distanceFromCenter);
        float3 densityNoiseCoords = float3(RotatedBy(coords - 0.5, distanceFromCenter * -15.1 + spinTime) * 3 + 0.5, z);
        float density = Noise3(densityNoiseCoords) * distanceFade * 6;
        
        // Reduse density in regions where there are stars twinkling, to brighten them up a bit.
        density -= twinkle * float4(z, z * 0.56 + 0.13, 0.5, 1) * twinkleBrightness;
        
        // Amplify the distance fade, making it so that the ends of the galaxy rapidly diminish.
        density += pow(distanceFade, edgeSpiralArmFadeExponent);
        
        totalDensity += density;
    }
    
    // Calculate the base color of the galaxy based on a similar time-based spiral arm calculation, interpolating through an inputted palette.
    float colorSwirl = distance(coords, 0.5) * -10 + spinTime;
    float hue = tex2D(noiseTextureA, RotatedBy(coords - 0.5, colorSwirl) * 2 + 0.5);
    float3 hueColor = PaletteLerp(hue);
    float4 colorExponent = 1 - float4(hueColor * 0.8, 0);
    float4 baseColor = exp(totalDensity * -colorExponent);
    
    // Calculate an inner glow value, to make the center of the galaxy extra bright.
    float innerGlow = smoothstep(0.175, 0, distance(coords, 0.5) / scale) * 1.2 + smoothstep(0.33, 0, distance(coords, 0.5) / scale) * 0.3;
    
    // Combine everything together.
    return (baseColor + innerGlow * 0.333) * galaxyBrightnessFactor;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(baseTexture, coords) * sampleColor;
    
    // Calculate the background color based on warped perlin noise.
    float warpTime = globalTime * 0.11;
    float warp = tex2D(backgroundTexture, coords * 4 + float2(0, warpTime)) * 0.06;
    warp = tex2D(backgroundTexture, coords * 3 + float2(warpTime, 0) + warp) * 0.09;    
    color += float4(0.09, 0, 0.16, 0) * tex2D(backgroundTexture, coords * 1.3 + warp);
    color += float4(0.01, 0, 0.11, 0) * tex2D(backgroundTexture, coords * 2.1 - warp);
    color -= float4(1, 1, 1, 0) * tex2D(backgroundTexture, coords * 1.9).r * 0.2;
    
    // Calculate the galaxy color.
    color += CalculateGalaxyColor(coords, 1);
    
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}