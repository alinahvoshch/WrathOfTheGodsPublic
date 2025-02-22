sampler screenTexture : register(s0);
sampler dropNoiseTexture : register(s1);

float globalTime;
float animationCompletion;
float dropletDissipateSpeed;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 baseCoords = coords;
    float4 baseColor = tex2D(screenTexture, coords);
    
    // Round coordinates. This is necessary to ensure that the droplets have a consistent shape across the effective screen grid.
    float2 resolution = float2(20, 10);
    float2 roundedCoords = round(coords * resolution - 0.2) / resolution;
    roundedCoords += float2(tex2D(dropNoiseTexture, roundedCoords).r, tex2D(dropNoiseTexture, roundedCoords + 0.4).r) * 0.4;
    
    // Calculate noise.
    float noise = tex2D(dropNoiseTexture, roundedCoords);
    
    // Calculate the X and Y angles responsible for the shape of the droplets.
    // This relies on the position of the pixel and a little bit of noise.
    // The stronger the noise is intensified, the lumpier the droplets will be.
    float2 angles = coords * resolution * 6.283 + (tex2D(dropNoiseTexture, coords).xy - 0.5) * 2;
    
    float2 threshold = sin(angles) - frac(animationCompletion * dropletDissipateSpeed + noise) * 0.8;
    
    // Calculate the droplet color by sampling the screen at an offset position in accordance with the angles.
    float4 rainDropColor = tex2D(screenTexture, coords + cos(angles) * 0.2);
    rainDropColor.gb *= 0.1;
    rainDropColor.r *= 3;
    
    return lerp(baseColor, rainDropColor, threshold.x + threshold.y - noise * 2.5 >= smoothstep(0.5, 1, animationCompletion) * 0.5 + smoothstep(0.15, 0, animationCompletion) * 0.6 + 0.2);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
