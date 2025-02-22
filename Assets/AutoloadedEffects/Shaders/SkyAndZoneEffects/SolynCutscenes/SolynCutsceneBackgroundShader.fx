sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float brightnessThresholding;
float wavinessFactor;
float flowSpeed;
float darkeningFactor;
float twinkleSpeed;
float2 pixelationFactor;

float Hash12(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 30000);
}

float CalculateStarBrightness(float2 coords, float cutoffThreshold)
{
    float brightness = Hash12(coords);
    bool twinkleCanAppear = brightness >= cutoffThreshold;
    
    // Apply extremely strong squashing to the brightness value relative to the cutoff threshold, ensuring twinkles are scant.
    brightness = pow((brightness - cutoffThreshold) / (1 - cutoffThreshold), 19);
    
    // Apply a twinkle effect to the brightness, to make the stars more varied and interesting.
    float twinkle = cos(globalTime * twinkleSpeed * 6.283 + brightness * 1485 - coords.x * 100 + coords.y * 100) * 0.5;
    brightness *= 1 + twinkle;
    
    return saturate(brightness * 5) * twinkleCanAppear;
}

float2 SampleDarknessLayer(float2 coords, float zoom, float offset)
{
    float time = globalTime * 0.8;
    
    // Determine the starting point of the darkness layer, starting roughly at the top right of the game scene.
    float2 sourceOrigin = float2(1.1, offset);
    float2 offsetFromSource = coords - sourceOrigin;

    // Calculate polar coordinates relative to the source origin.
    float2 polar = float2(atan2(offsetFromSource.y, offsetFromSource.x) / 6.283 + 0.5, length(offsetFromSource));
    
    // Calculate offset values for the noise.
    // These dictate waviness and forward moving flow in the motion of the darkness layer.
    float2 wave = float2(sin(polar.y * 17.5 - time + offset * 100) * wavinessFactor, 0);
    float2 flow = float2(0, time * -flowSpeed);
    
    // Calculate noise from the polar coordinates, with a single instance of FBM-like self-affection for extra detail.
    float darkening = tex2D(noiseTexture, polar * float2(1, zoom * 0.1) * 3);
    darkening = tex2D(noiseTexture, polar * float2(1, zoom * 0.08) * 2 + wave + flow + darkening * 0.019);
    
    // Threshold the darkening value.
    darkening = smoothstep(brightnessThresholding, 1, darkening);

    return darkening;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate pixelated coordinates for use with the twinkling effect, to ensure consistent sizing with the stars.
    float2 pixelatedCoords = round(coords / pixelationFactor) * pixelationFactor;
    
    // Use the pixelated coordinates to calculate star brightness values.
    float twinkle = CalculateStarBrightness(pixelatedCoords, 0.98) + CalculateStarBrightness(pixelatedCoords * 0.5, 0.99);
    
    // Combine three
    float darkening = (SampleDarknessLayer(coords, 1, 0) + SampleDarknessLayer(coords, 0.5, 0.12) + SampleDarknessLayer(coords, 1.5, -0.12)) * 0.333;
    
    // Calculate the background color, using the sample color as a baseline and then subtracting by the darkening value.
    float3 backgroundColor = sampleColor.rgb - darkening * darkeningFactor;
    
    // Mute the twinkle if it's above a brighter patch of space, to help reinforce the idea that the brighter patches are like clouds layering over things.
    twinkle *= pow(darkening, 1.56) * darkeningFactor * 2.85;
    
    return float4(backgroundColor + twinkle * sampleColor.a, 1) * sampleColor.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}