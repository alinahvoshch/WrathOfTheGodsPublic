sampler noiseTexture : register(s1);
sampler scleraTexture : register(s2);

float globalTime;
float openEyeInterpolant;
float irisScale;
float pupilScale;
float2 size;
float3 irisColorA;
float3 irisColorB;
float2 pupilOffset;
float3 baseScleraColor;

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

float4 CalculateScleraColor(float2 coords)
{
    // Calculate polar coordinates.
    float2 scaledCoords = (coords - 0.5) * float2(size.x / size.y, 1) + 0.5;
    float2 offsetFromCenter = scaledCoords - 0.5;
    float distanceFromCenter = length(offsetFromCenter);
    float2 polar = float2(atan2(offsetFromCenter.y, offsetFromCenter.x) / 6.283 + 0.5, distanceFromCenter);
    
    float4 scleraColor = float4(baseScleraColor, 1);
    
    // Make sclera colors brighter near the center.
    scleraColor += exp(distanceFromCenter * -9.5) * 1.4;
    
    // Bias colors towards a blood-like color based on noise, as a contrast to the general cosmic color of the sclera.
    float2 bloodNoiseCoords = tex2D(noiseTexture, polar * float2(2, 0.6) + float2(0, globalTime * -0.02));
    float bloodInterpolant = smoothstep(0, 0.6, bloodNoiseCoords);
    scleraColor = lerp(scleraColor, float4(0.4, 0.05, 0, 1), bloodInterpolant);
    
    return saturate(scleraColor) + tex2D(scleraTexture, polar + float2(0, globalTime * -0.01));
}

float4 CalculateIrisAndPupilColor(in float2 coords, float squishInterpolant)
{
    // Immediately offset coords in accordance with the pupil offset, so that it can move around a little bit.
    coords -= pupilOffset;
    
    // Calculate polar coordinates, with an inbuilt outward scroll.
    float2 scaledCoords = (coords - 0.5) * float2(size.x / size.y, 1) + 0.5;
    float2 offsetFromCenter = scaledCoords - 0.5;
    float distanceFromCenter = length(offsetFromCenter) / squishInterpolant / irisScale;
    float2 polar = float2(atan2(offsetFromCenter.y, offsetFromCenter.x) / 6.283 + 0.5, distanceFromCenter);
    polar.y -= globalTime * 0.075;
    
    // Calculate the hue interpolant of the iris.
    // This will dictate which base color the iris should be at a given pixel.
    float irisHueInterpolant = tex2D(noiseTexture, polar * float2(6, 2));
    
    // Calculate how much the iris should be darkened as a 0-1 interpolant.
    // This is used to give mild dark accents to the iris colors.
    float irisEdgeDarkening = smoothstep(0.15, 0.19, distanceFromCenter);
    float irisDarkening = irisEdgeDarkening;
    
    // Calculate iris brightening.
    // This is:
    // 1. Influenced by a highlight to the top left of the pupil
    // 2. Muted by the aforementioned iris darkening calculation
    // 3. Affected by noise whose influence becomes stronger the further out it is from the center.
    float noiseSharpness = 3 - distanceFromCenter * 8;
    float distanceFromHighlight = distance(coords, float2(0.35, 0.45));
    float irisHighlight = exp(distanceFromHighlight * -17);
    float irisNoiseBrightening = pow(tex2D(noiseTexture, coords * 0.6 + float2(0.35, 0)), noiseSharpness);
    float irisBrightening = irisNoiseBrightening + irisHighlight;
    irisBrightening *= (1 - irisDarkening) * 2;
    
    // Combine things together for the iris color.
    float3 irisColor = saturate(lerp(irisColorA, irisColorB, irisHueInterpolant) + irisBrightening - irisDarkening * 0.6);
    float irisOpacity = smoothstep(0.3, 0.18, distanceFromCenter);
    
    // Determine how much the pupil should darken things.
    float pupilDarkening = smoothstep(0.08, 0.04, distanceFromCenter / pupilScale);
    
    return float4(irisColor - pupilDarkening, 1) * irisOpacity;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate erasure interpolants.
    // These dictate the shape of the eye at the top and bottom half, effectively cutting out the rectangle that this shader is applied to
    // and creating the eye shape.
    float upperErasureThreshold = 0.5 - pow(QuadraticBump(pow(coords.x, 1.15)), 0.7) * openEyeInterpolant * 0.4;
    float lowerErasureThreshold = QuadraticBump(coords.x) * openEyeInterpolant * 0.33 + 0.5;
    float squishInterpolant = smoothstep(0, 0.8, openEyeInterpolant);
    float upperErasureInterpolant = smoothstep(0.92, 1, coords.y / upperErasureThreshold * squishInterpolant);
    float lowerErasureInterpolant = smoothstep(1, 0.92, coords.y / lowerErasureThreshold / squishInterpolant);
    float erasureOpacity = sqrt(upperErasureInterpolant * lowerErasureInterpolant);
    
    // Calculate the color of the sclera, iris, and pupil, and combine them together.
    float4 scleraColor = CalculateScleraColor(coords);
    float4 irisColor = CalculateIrisAndPupilColor(coords, squishInterpolant);
    float4 result = lerp(scleraColor, irisColor, irisColor.a);
    
    // Darken colors based on the erasure opacity.
    // This will effectively make translucent colors darker, to help sell the lighting a bit with some faux shadows.
    result.rgb -= pow(1 - erasureOpacity, 0.6) * 2;
    
    return result * erasureOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}