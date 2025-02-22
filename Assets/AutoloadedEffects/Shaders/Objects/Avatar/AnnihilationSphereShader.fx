sampler baseTexture : register(s0);
sampler uvOffsetNoiseTexture : register(s1);

float time;
float distortionStrength;
float maxLensingAngle;
float blackRadius;
float2 sourcePosition;
float2 aspectRatioCorrectionFactor;
float3 accretionDiskFadeColor;
float4 brightColor;

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float CalculateGravitationalLensingAngle(float2 coords)
{
    // Calculate how far the given pixels is from the source of the distortion. This autocorrects for the aspect ratio resulting in
    // non-square calculations.
    float distanceToSource = max(distance((coords - 0.5) * aspectRatioCorrectionFactor + 0.5, sourcePosition), 0);
    
    // Calculate the lensing angle based on the aforementioned distance. This uses distance-based exponential decay to ensure that the effect
    // does not extend far past the source itself.
    float gravitationalLensingAngle = distortionStrength * maxLensingAngle * exp(-distanceToSource / blackRadius * 2);
    return gravitationalLensingAngle;
}

float4 ApplyColorEffects(float4 color, float gravitationalLensingAngle, float2 coords, float2 distortedCoords)
{
    // Calculate offset values based on noise. Points sampled from this always give back a unit vector's components in the Red and Green channels.
    float2 uvOffset1 = tex2D(uvOffsetNoiseTexture, distortedCoords + float2(0, time * 0.8));
    float2 uvOffset2 = tex2D(uvOffsetNoiseTexture, distortedCoords * 0.4 + float2(0, time * 0.7));
    
    // Calculate color interpolants. These are used below.
    // The black hole uses a little bit of the UV offset noise for calculating the edge boundaries. This helps make the effect feel a bit less
    // mathematically perfect and more aesthetically interesting.
    float offsetDistanceToSource = max(distance((coords - 0.5) * aspectRatioCorrectionFactor + 0.5, sourcePosition + uvOffset1 * 0.008), 0);
    float blackInterpolant = smoothstep(blackRadius, blackRadius * 0.85, offsetDistanceToSource);
    float brightInterpolant = pow(smoothstep(blackRadius * (1.01 + uvOffset2.x * 0.04), blackRadius * 0.97, offsetDistanceToSource), 1.2) * 0.6 + pow(gravitationalLensingAngle * 7.777 / maxLensingAngle, 13);
    
    // Interpolate towards the bright color first.
    color = lerp(color, brightColor + uvOffset2.y * brightColor.a * 0.9, saturate(brightInterpolant) * distortionStrength);
    
    // Lastly, place the black hole in the center above everything.
    color = lerp(color, float4(0, 0, 0, 1), blackInterpolant * distortionStrength);
    
    return color;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate the gravitational lensing angle and the coordinates that result from following its rotation.
    // This roughly follows the mathematics of relativistic gravitational lensing in the real world, albeit with a substitution for the impact parameter:
    // https://en.wikipedia.org/wiki/Gravitational_lensing_formalism
    // Concepts such as the speed of light, the gravitational constant, mass etc. aren't really necessary in this context since those physics definitions do not
    // exist in Terraria, and given how extreme their values are it's possible that using them would result in floating-point imprecisions.
    float gravitationalLensingAngle = CalculateGravitationalLensingAngle(coords);
    float2 distortedCoords = RotatedBy(coords - 0.5, gravitationalLensingAngle) + 0.5;
    
    // Calculate the colors based on the above information.
    return ApplyColorEffects(0, gravitationalLensingAngle, coords, distortedCoords);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}