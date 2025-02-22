sampler reflectionNoiseTexture : register(s1);
sampler psychedelicTexture : register(s2);

float localTime;
float stringCount;
float twoPi = 6.283185;
float psychedelicRingZoom;
float psychedelicPulseRadius;
float psychedelicPulseAnimationSpeed;
float psychedelicWarpInfluence;
float2 pixelationFactor;

// This is NOT OK.
// Shaders were NOT built to construct complex objects!
// But since when has that ever stopped me...

float4 CalculateStringColor(float2 coords, float radius)
{
    // Combine a bunch of circles together to create cool radial patterns.
    float minDistanceToCircle = 1;
    float clampedStringCount = clamp(stringCount, 0, 15);
    for (int i = 0; i < clampedStringCount; i++)
    {
        float angle = twoPi * i / clampedStringCount;
        
        float2 circleCenter = 0.5 + float2(cos(angle), sin(angle)) * (0.455 - radius);
        float distanceToCircleCenter = distance(coords, circleCenter);
        float distanceToCircleEdge = distance(distanceToCircleCenter, radius);

        minDistanceToCircle = min(minDistanceToCircle, distanceToCircleEdge);
    }
    
    // Calculate a rainbow color that adorns the strings.
    // The further out the pixel is from the center, the more pastel-ized the color becomes, fading to white.
    float distanceFromCenter = distance(coords, 0.5);
    float stringBrightness = smoothstep(1, 0.25, minDistanceToCircle / 0.004);
    float pastelInterpolant = smoothstep(0.3, 0.45, distanceFromCenter);
    float4 stringColor = saturate(0.1 / tex2D(psychedelicTexture, float2(0.3, distanceFromCenter * 0.3 - localTime * 0.04)));
    stringColor = lerp(stringColor, 1, pastelInterpolant);
    stringColor.a = 1;
    
    return stringColor * stringBrightness;
}

float4 CalculateRingColor(float4 originalColor, float4 sampleColor, float2 coords)
{
    // Calculate the ring interpolant. This will be used to compose the shape of the outer ring.
    float distanceFromCenter = distance(coords, 0.5);
    float distanceFromEdge = distance(distanceFromCenter, 0.46);
    float unclampedRingInterpolant = distanceFromEdge / 0.023;
    float ringInterpolant = smoothstep(1, 0.7, unclampedRingInterpolant);
    
    // Use the aforementioned ring interpolant to create a ring color.
    float ringBrightness = tex2D(reflectionNoiseTexture, distanceFromCenter * 1.9).r * ringInterpolant;
    float4 baseRingColor = float4(ringBrightness, ringBrightness, ringBrightness, 1) * sampleColor * smoothstep(0, 0.7, ringInterpolant);
    baseRingColor += baseRingColor.a * smoothstep(0.33, 1, ringBrightness) * 0.3;
    
    // Calculate light data.
    float2 directionFromCenter = normalize(coords - 0.5);
    float3 normal = normalize(float3(directionFromCenter * 0.5, 0.5));
    float3 lightDirection = normalize(float3(0, 0, 1));
    float3 lightSourcePosition = float3(0, 0, 0);
    
    // Calculate the influence of specular lighting.
    float3 directionFromLightSource = normalize(float3(coords, 0) - lightSourcePosition);
    float3 reflectionDirection = normalize(reflect(lightDirection, normal));
    float specularLighting = saturate(dot(directionFromLightSource, reflectionDirection));
    
    // Calculate the influence of diffuse lighting.
    float diffuseLighting = saturate(dot(normal, -directionFromLightSource));
    
    float light = pow(specularLighting, 6) * 0.85 + diffuseLighting;
    
    return lerp(originalColor, baseRingColor * (1 + light * 0.2), baseRingColor.a);
}

float4 CalculatePsychedelicColor(float2 coords)
{
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distance(coords, 0.5));
    float4 color = 0;
    float2 warp = tex2D(reflectionNoiseTexture, polar * float2(1, 0.5) - localTime * 0.04).rg * 0.1;
    
    // Standard FBM with a bit of extra spice added. Nothing special.
    for (int i = 0; i < 4; i++)
    {
        float2 scrollDirection = float2(sin(i * 2) - 0.2, 0);
        float2 localCoords = coords * pow(1.2, i) + scrollDirection * localTime * 0.1 + warp;
        color += tex2D(psychedelicTexture, localCoords) / pow(1.6, i);
        warp = color.rb * psychedelicWarpInfluence;
    }
    
    // Add some white to the center of the psychedelic blob, to serve as a center for the dreamcatcher.
    // For animation purposes, a pulsation effect is applied to go along with this.
    float pulse = frac(localTime * psychedelicPulseAnimationSpeed);
    color += smoothstep(0.12, 0.04, polar.y);
    color += smoothstep(0.12, 0.04, polar.y - pulse * psychedelicPulseRadius) * (1 - pulse);
    
    return color;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Pixelate things.
    coords = round(coords / pixelationFactor) * pixelationFactor;
    
    // Combine the various effects together.
    float4 stringColor = CalculateStringColor(coords, 0.417) + CalculateStringColor(coords, 0.227);
    float4 ringColor = CalculateRingColor(stringColor, sampleColor, coords);
    float distanceFromCenter = distance(coords, 0.5);
    float4 psychedelicColor = CalculatePsychedelicColor((coords - 0.5) * psychedelicRingZoom + 0.5) * smoothstep(0.4, 0.3, distanceFromCenter);
    
    return saturate(ringColor + psychedelicColor) * sampleColor.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}