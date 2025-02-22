sampler baseTexture : register(s0);
sampler forcefieldFlowNoiseTexture : register(s1);

float bottomFlattenInterpolant;
float globalTime;
float shapeInstability;
float flashInterpolant;
float forcefieldPaletteLength;
float4 forcefieldPalette[15];

float4 PaletteLerp(float interpolant, float gradientCount, float4 gradient[15])
{
    int startIndex = clamp(frac(interpolant) * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Apply distortion effects at the bottom half of the forcefield.
    coords.y = lerp(coords.y, 1, smoothstep(0.5, 0.7, coords.y) * bottomFlattenInterpolant);
    
    // WE LOVE POLAR COORDINATES AROUND HERE!
    float distanceFromCenter = distance(coords, 0.5);
    float2 polar = float2(distanceFromCenter, atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5);
    
    // Add a bit of noise-induced instability to the distance from center value, to help make the forcefield shape not feel like an unnaturally perfect sphere.
    distanceFromCenter += tex2D(forcefieldFlowNoiseTexture, polar * 2 + float2(0, globalTime)) * shapeInstability;
    
    float distanceFromEdge = distance(distanceFromCenter, 0.48);
    
    // Determine the base glow. This is strongest at the edges of the forcefield, but gradually fades a bit when going inward.
    float4 baseGlow = smoothstep(0.02, 0, distanceFromEdge) + pow(smoothstep(0.1, 0.47, distanceFromCenter), 5.5) * (distanceFromCenter <= 0.5);
    
    // Make the bottom of the forcefield translucent, so that it looks more like something the player would be able to enter from below.
    baseGlow *= lerp(0.45, 1, smoothstep(0.75, 0.5, coords.y));
    
    // Calculate the forcefield color.
    float hue = tex2D(forcefieldFlowNoiseTexture, polar * 2 - float2(globalTime * 1.4, 0)) + (1 - baseGlow.r) * 0.4 - globalTime * 0.9;
    float4 forcefieldColor = PaletteLerp(hue, forcefieldPaletteLength, forcefieldPalette);    
    float4 color = forcefieldColor;
    
    color += float4(0.3, -0.3, 1, 0) * smoothstep(0.5, 0.1, distanceFromCenter) * 5;
    
    return color * sampleColor * baseGlow.a * (1 + flashInterpolant);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}