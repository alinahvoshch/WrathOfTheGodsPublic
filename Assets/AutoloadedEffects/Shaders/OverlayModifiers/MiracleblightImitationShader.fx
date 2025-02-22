sampler2D baseTexture : register(s0);
sampler2D noiseTexture : register(s1);

float baseErasureThreshold;
float globalTime;
float zoom;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(baseTexture, coords);
    if (!any(color))
        return color;

    // Temporal drift to make the sprite slide through the noise texture
    float2 drift = float2(5 * sin(0.04165 * globalTime), 1.1 * globalTime);

    float2 noiseMapTexCoords = coords * float2(10, 1) + drift;
    float4 noiseColor = (tex2D(noiseTexture, noiseMapTexCoords * zoom * 1.1) + tex2D(noiseTexture, noiseMapTexCoords * zoom * 0.76)) * 0.5;

    // Define thresholds for total pixel erasure and glowing lines.
    //
    // Rapidly flickering sinewave produced by Desmos, loosely based on the Weierstrass function
    // (infinitely sharp vague sinewave, periodic, continuous everywhere but differentiable nowhere)
    // https://en.wikipedia.org/wiki/Weierstrass_function
    float flickerOne = cos(globalTime * 7) * 0.05;
    float flickerTwo = cos(globalTime * 31) * 0.06;
    float flickerThree = sin(globalTime * 167) * 0.04;
    float fullErasureThreshold = baseErasureThreshold + flickerOne + flickerTwo + flickerThree;
    float glowThreshold = fullErasureThreshold - 0.1;

    // If the noise over the erasure threshold, completely erase this pixel.
    if (noiseColor.r > fullErasureThreshold)
    {
        color.rgba = 0;
    }

    // Otherwise, if it's over the slightly lower threshold, replace it with a bright color.
    else if (noiseColor.r > glowThreshold)
    {
        // Ensure it accounts for the original alpha.
        color = float4(0.4902, 1, 0, 1) * color.a;
    }

    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}