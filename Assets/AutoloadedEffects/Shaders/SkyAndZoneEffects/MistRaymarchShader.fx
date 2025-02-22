sampler baseTexture : register(s0);
sampler densityTextureA : register(s1);
sampler densityTextureB : register(s2);
sampler metaballTarget : register(s3);
sampler psychedelicTexture : register(s4);

float globalTime;
float minDropletSize;
float maxDropletSize;
float stepIncrement;
float brightnessAccentuationBiasExponent;
float brightnessAccentuationBiasFactor;
float saturationAccentuationBiasExponent;
float saturationAccentuationBiasFactor;
float3 lightPosition;
float3 lightWavelengths;

float CalculateMistDensity(float3 samplePosition)
{
    samplePosition.xy *= 1.9;
    samplePosition.x += globalTime / pow(abs(samplePosition.z), 0.3) * 0.02;
    samplePosition.y *= 0.5;
    samplePosition.z *= 75;
    
    float warpAngle = tex2D(densityTextureB, samplePosition.zy * 0.4 + globalTime * 0.004) * 16;
    float2 warp = float2(cos(warpAngle), sin(warpAngle));
    samplePosition.xy += warp * 0.05;
    
    float mistNoise = sqrt(tex2D(densityTextureA, samplePosition.xy - samplePosition.z) * tex2D(densityTextureB, samplePosition.xz + samplePosition.y + 0.9));
    
    return smoothstep(0.39, 1, mistNoise);
}

float4 CalculateLightScattering(float3 samplePosition, float3 rayDirection)
{
    float3 lightDirection = normalize(lightPosition - samplePosition);
    float angle = acos(dot(rayDirection, lightDirection));
    
    float dropletSizeInterpolant = pow(CalculateMistDensity(samplePosition * 0.3), 1.2);
    float dropletSize = lerp(minDropletSize, maxDropletSize, dropletSizeInterpolant);
    
    float3 diffraction = sin(angle * 30) * dropletSize / lightWavelengths;    
    float3 resultingColor = sin(diffraction * 6.283);
    
    return float4(resultingColor, 1);
}

float4 CalculateRaymarchedLight(float3 origin, float3 rayDirection)
{
    float4 light = 0;
    
    float overallDensity = 1e-6;
    float stepCount = 5;

    // Raymarch through the volume, calculating mist density at each point.
    float3 rayIncrement = rayDirection / stepCount * stepIncrement;
    for (int i = 0; i < stepCount; i++)
    {
        float3 samplePosition = origin + rayIncrement * i;
        
        float sampleDensity = CalculateMistDensity(samplePosition);
        float4 scatteredLight = CalculateLightScattering(samplePosition, rayDirection);
        light += sampleDensity * scatteredLight;
        overallDensity += sampleDensity;
    }
    
    // Apply attenunation in a manner similar to tone-mapping.
    light *= exp(-overallDensity);
    
    return light;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 light = CalculateRaymarchedLight(float3(coords, -0.6), float3(0, 0, 1));
    
    float greyscale = dot(light.rgb, float3(0.3, 0.6, 0.1));
    float saturation = abs(light.r - greyscale) + abs(light.g - greyscale) + abs(light.b - greyscale);
    
    // Apply saturation biasing.
    light *= 1 + pow(saturation, saturationAccentuationBiasExponent) * saturationAccentuationBiasFactor;
    
    // Apply brightness biasing.
    light = lerp(light, light.a * 0.75, pow(1 - saturation, brightnessAccentuationBiasExponent) * brightnessAccentuationBiasFactor);
    
    // Bias towards psychedelic colors in general.
    float2 psychedelicCoords = coords * 0.7 + light.r * 0.1;
    float4 psychedelicColor = lerp(tex2D(psychedelicTexture, psychedelicCoords), 0.9, 0.5);
    light = lerp(light, psychedelicColor, 0.6 - saturation);
    
    float4 metaballContents = tex2D(metaballTarget, coords);
    metaballContents.a = 0;
    
    light = lerp(light, tex2D(psychedelicTexture, coords * 2 + light.xy * 0.1), 0.5);
    
    float4 color = tex2D(baseTexture, coords);
    return color * sampleColor * light * metaballContents * lerp(2, 4, saturation);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}