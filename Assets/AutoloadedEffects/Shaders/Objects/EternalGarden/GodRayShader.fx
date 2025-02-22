sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float gradientCount;
float2 sourcePosition;
float2 sceneArea;
float3 gradient[8];

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(frac(interpolant) * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Calculate a modified radial value based on the offset from the top of the scene.
    float2 offsetFromSource = (sourcePosition - position.xy) / sceneArea;
    float2 radial = float2(atan2(offsetFromSource.y, offsetFromSource.x) / 6.283 + 0.5, length(offsetFromSource));
    radial.x *= 3;
    radial.y *= 0.015;
    
    // Combine two noise values in alternation together to compose the texture of the glow.
    float scrollTime = globalTime * 0.32;
    float noiseA = tex2D(noiseTexture, radial * float2(1, 1.3) + float2(-0.04, 0) * scrollTime);
    float noiseB = tex2D(noiseTexture, radial * float2(2, 1.5) + float2(0.04, 0) * scrollTime);;
    float combinedNoise = sqrt(noiseA * noiseB) - pow(noiseA, 0.8) * 0.25;
    
    // Calculate the positional fade as a means of ensuring that the colors fade away at the edges of the rectangle that composes the rays.
    float bottomFade = smoothstep(0.94, 0.9, coords.y) * smoothstep(0, 0.7, coords.y);
    float horizontalEdgeFade = pow(QuadraticBump(coords.x), 2.3);
    float positionalFade = bottomFade * horizontalEdgeFade;
    
    // Combine everything together.
    float opacity = positionalFade * sampleColor.a * combinedNoise * 5;
    return float4(PaletteLerp(combinedNoise), 1) * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}