sampler baseTexture : register(s1);

float samplePointCount;
float detail;
float time;
float noiseExponent;
float noiseSelfInfluence;
float noiseUndulationIntensity;
float gradientCount;
float foreshortening;
float2 cameraViewExaggeration;
float3 planeOrigin;
float3 planeNormal;
float3 gradient[7];

float3 PaletteLerp(float interpolant)
{
    float startIndex = clamp(frac(interpolant) * gradientCount, 0, gradientCount - 1);
    float endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Use exaggerated UV coordinates to calculate the camera ray direction.
    // This exaggeration allows for a broader (or narrower) coverage of the aurora.
    float2 exaggeratedCoords = (coords - 0.5) * cameraViewExaggeration;
    float3 cameraDirection = normalize(float3(exaggeratedCoords, 1 / foreshortening));
    
    // WE LOVE RAYMARCHING!!
    float previousNoise = 0;
    float hueInterpolant = 0;
    float clampedSamplePointCount = clamp(samplePointCount, 0, 15);
    for (int i = 0; i < clampedSamplePointCount; i++)
    {
        // Step forward from the origin in accordance with the camera direction and the current loop iteration.
        float3 samplePoint = cameraDirection * i * detail;
        
        // Calculate how far up the sample point is from the plane, and then project the sample point onto the plane.
        float distanceAbovePlane = dot(planeNormal, samplePoint - planeOrigin);        
        float3 planeCoords = samplePoint - planeNormal * distanceAbovePlane;
        
        // Calculate a horizontal undulation. This gives the aurora its silken look.
        // If the undulation intensity is set to 0, the aurora will simply scroll forward without much variation in shape.
        float noiseUndulationOffset = cos(time * 0.6 + samplePoint.z * 6.283) * planeCoords.z * noiseUndulationIntensity;
        
        // Use the previous noise value to offset things. This is REALLY important for giving the aurora a sense of micro-motion, allowing repeated noise
        // to influence itself and cause chaotic details that allow the aurora to look more fluid-like, rather than just an obvious noise texture scroll.
        float2 noiseCoordsOffset = float2(previousNoise * noiseSelfInfluence + noiseUndulationOffset, time * i * 0.01);
        
        // Use the plane projection point, modified via the loop iteration variable for variety, along with the offset to determine the final noise coordinate.
        float2 noiseCoords = planeCoords.xy * (i * 0.026 + 0.1) + noiseCoordsOffset;        
        
        // Use the above coordinate calculations to determine characteristic noise.
        // This is extremely important for determining the overall look of the aurora, dictating how the color bands are distributed.
        // Furthermore, as a consequence of this, the choice of texture matters considerably.
        float noise = pow(tex2D(baseTexture, noiseCoords), noiseExponent);
        
        // Increment the hue interpolant based on the noise, along with the height above the plane.
        // The inclusion of height in this calculation allows for the culling of below-the-plane sample points, and helps in creating bright bands.
        hueInterpolant += saturate((distanceAbovePlane - 0.3) * noise);
        previousNoise = noise;        
    }
    
    // Average out hue interpolant contributions from the above loop.
    hueInterpolant /= clampedSamplePointCount;
    
    // Map the modified hue interpolant onto a specified palette that composes the aurora.
    hueInterpolant = smoothstep(0.08, 0.9, hueInterpolant);
    hueInterpolant = clamp(pow(hueInterpolant, 2) * 3.5, 0, 0.8);
    return float4(PaletteLerp(hueInterpolant), 0) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}