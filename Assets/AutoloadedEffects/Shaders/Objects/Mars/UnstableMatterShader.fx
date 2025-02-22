sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float gradientCount;
float warpSmoothness;
float swirlRingQuantityFactor;
float swirlAnimationSpeed;
float swirlProminence;
float innerGlowIntensity;
float centerDarkeningHarshness;
float edgeWarpIntensity;
float edgeWarpAnimationSpeed;
float2 size;
float3 gradient[5];

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

float2 CalculateSphereCoords(float2 coords)
{
    // Calculate the distance to the center of the star. This is magnified a bit for intensity purposes in later equations.
    float2 coordsNormalizedToCenter = (coords - 0.5) * 2;
    float distanceFromCenterSqr = dot(coordsNormalizedToCenter, coordsNormalizedToCenter) * 2;
    
    // Calculate coordinates relative to the sphere.
    // This pinch factor effectively ensures that the UVs are relative to a circle, rather than a rectangle.
    // This helps SIGNIFICANTLY for making the texturing look realistic, as it will appear to be traveling on a
    // sphere rather than on a sheet that happens to overlay a circle.
    float spherePinchFactor = (1 - sqrt(abs(1 - distanceFromCenterSqr))) / distanceFromCenterSqr + 0.001;
    
    // Exaggerate the pinch slightly.
    spherePinchFactor = pow(spherePinchFactor, 1.95);
    
    return frac((coords - 0.5) * spherePinchFactor + 0.5);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Pixelate everything.
    float2 pixelationFactor = 1.5 / size;
    coords = floor(coords / pixelationFactor) * pixelationFactor;
    
    // Calculate basic polar coordinates.
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distance(coords, 0.5));
    
    // Determine how much distance calculations should be warped.
    // This is affected by the unmodified distance from the center, to ensure that warping happens primarily at the edges, without interfering with the swirly shapes on the inside.
    float distanceWarp = tex2D(noiseTexture, polar + globalTime * float2(0, -edgeWarpAnimationSpeed)) * polar.y * edgeWarpIntensity;
    
    float distanceFromCenter = distance(coords, 0.5) + distanceWarp;
    float distanceFromEdge = distance(distanceFromCenter, 0.3);    
    float2 sphereCoords = CalculateSphereCoords(coords);
    
    // Apply FBM to calculate a warp offset.
    float2 warp = 0;
    for (int i = 0; i < 4; i++)
    {
        float2 scrollDirection = float2(sin(i * 2) - 0.2, 0);
        float2 localCoords = sphereCoords * pow(1.3, i) + scrollDirection * globalTime * 0.7 + warp;
        warp = tex2D(noiseTexture, localCoords) / pow(warpSmoothness, i) * 0.4;
    }
    
    // Swirl the sphere coordinates to include a swirl towards the center.
    float swirlAngle = (distanceFromCenter + globalTime * 0.4) * swirlRingQuantityFactor;
    float2 finalCoords = frac(sphereCoords + warp + globalTime * swirlAnimationSpeed);
    finalCoords = RotatedBy(finalCoords - 0.5, swirlAngle) + 0.5;
    
    // Calculate the hue value, based on a combination of noise and distance to the edge.
    float hue = tex2D(noiseTexture, finalCoords);    
    hue *= pow(0.04 / distanceFromEdge, 1 / swirlProminence);
    hue += smoothstep(0.27, 0.36, distanceFromCenter) * 0.8;
    
    float3 color = PaletteLerp(saturate(hue));
    
    // Darken colors as they get closer to the center, to add general depth.
    color -= smoothstep(0.3, 0.05, distanceFromCenter) * centerDarkeningHarshness;
    
    // Make colors at the close center brighter.
    color += pow(innerGlowIntensity / distanceFromCenter, 1.2);
    
    float opacity = smoothstep(0.384, 0.32, distanceFromCenter);
    color.rb += smoothstep(0.6, 0, opacity) * 0.95;
    
    return float4(color, 1) * sampleColor * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}