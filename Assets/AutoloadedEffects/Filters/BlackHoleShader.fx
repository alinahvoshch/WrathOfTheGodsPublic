sampler baseTexture : register(s0);
sampler uvOffsetNoiseTexture : register(s1);

sampler tintedTextureA : register(s2);
sampler tintedTextureB : register(s3);
sampler tintedTextureC : register(s4);
sampler tintedTextureD : register(s5);
sampler tintedTextureE : register(s6);

float globalTime;
float distortionStrength;
float maxLensingAngle;
float blackRadius;
float2 sourcePositions[5];
float2 aspectRatioCorrectionFactor;
float3 accretionDiskFadeColor;
float3 uColor;
float3 uSecondaryColor;

float InverseLerp(float from, float to, float x)
{
    return saturate((x - from) / (to - from));
}

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float CalculateGravitationalLensingAngle(float2 coords, float2 sourcePosition)
{
    // Calculate how far the given pixels is from the source of the distortion. This autocorrects for the aspect ratio resulting in
    // non-square calculations.
    float distanceToSource = max(distance((coords - 0.5) * aspectRatioCorrectionFactor + 0.5, sourcePosition), 0);
    
    // Calculate the lensing angle based on the aforementioned distance. This uses distance-based exponential decay to ensure that the effect
    // does not extend far past the source itself.
    float gravitationalLensingAngle = distortionStrength * maxLensingAngle * exp(-distanceToSource / blackRadius * 2);
    return gravitationalLensingAngle;
}

float4 ApplyColorEffects(float4 color, float gravitationalLensingAngle, float2 coords, float2 distortedCoords, float2 sourcePosition)
{
    // Calculate offset values based on noise. Points sampled from this always give back a unit vector's components in the Red and Green channels.
    float2 uvOffset1 = tex2D(uvOffsetNoiseTexture, distortedCoords + float2(0, globalTime * 0.8));
    float2 uvOffset2 = tex2D(uvOffsetNoiseTexture, distortedCoords * 0.4 + float2(0, globalTime * 0.7));
    
    // Calculate color interpolants. These are used below.
    // The black hole uses a little bit of the UV offset noise for calculating the edge boundaries. This helps make the effect feel a bit less
    // mathematically perfect and more aesthetically interesting.
    float offsetDistanceToSource = max(distance((coords - 0.5) * aspectRatioCorrectionFactor + 0.5, sourcePosition + uvOffset1 * 0.008), 0);
    float blackInterpolant = InverseLerp(blackRadius, blackRadius * 0.85, offsetDistanceToSource);
    float brightInterpolant = pow(InverseLerp(blackRadius * (1.01 + uvOffset2.x * 0.1), blackRadius * 0.97, offsetDistanceToSource), 1.6) * 0.6 + gravitationalLensingAngle * 7.777 / maxLensingAngle;
    float accretionDiskInterpolant = InverseLerp(blackRadius * 1.93, blackRadius * 1.3, offsetDistanceToSource) * (1 - brightInterpolant);
    
    // Calculate the inner bright color. This is the color used right at the edge of the black hole itself, where everything is burning due to extraordinary amounts of particle friction.
    float4 brightColor = float4(lerp(uColor, uSecondaryColor, uvOffset1.y), 1) * 2;
    
    // Interpolate towards the bright color first.
    color = lerp(color, brightColor, saturate(brightInterpolant) * distortionStrength);
    
    // Interpolate towards the accretion disk's color next. This is what is drawn as a bit beyond the burning bright edge. It is still heated, but not as much, and as such is closer to an orange
    // glow than a blazing yellowish white.
    color = lerp(color, float4(accretionDiskFadeColor, 1), accretionDiskInterpolant * distortionStrength);
    
    // Lastly, place the black hole in the center above everything.
    color = lerp(color, float4(0, 0, 0, 1), blackInterpolant * distortionStrength);
    
    return color;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float lensingAngles[5];
    float2 distortedCoords = coords;
    for (int i = 0; i < 5; i++)
    {
        // Calculate the gravitational lensing angle and the coordinates that result from following its rotation.
        // This roughly follows the mathematics of relativistic gravitational lensing in the real world, albeit with a substitution for the impact parameter:
        // https://en.wikipedia.org/wiki/Gravitational_lensing_formalism
        // Concepts such as the speed of light, the gravitational constant, mass etc. aren't really necessary in this context since those physics definitions do not
        // exist in Terraria, and given how extreme their values are it's possible that using them would result in floating-point imprecisions.
        float2 sourcePosition = sourcePositions[i];
        lensingAngles[i] = CalculateGravitationalLensingAngle(coords, sourcePosition);
        distortedCoords = RotatedBy(distortedCoords - 0.5, lensingAngles[i]) + 0.5;
    }
    
    float4 color = tex2D(tintedTextureA, distortedCoords) + tex2D(baseTexture, distortedCoords);
    
    for (i = 0; i < 5; i++)
    {
        float2 sourcePosition = sourcePositions[i];        
        float gravitationalLensingAngle = lensingAngles[i];
        float2 distortedCoords = RotatedBy(coords - 0.5, gravitationalLensingAngle) + 0.5;
    
        // Calculate the colors based on the above information.
        color = ApplyColorEffects(color, gravitationalLensingAngle, coords, distortedCoords, sourcePosition);
    }
    
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}