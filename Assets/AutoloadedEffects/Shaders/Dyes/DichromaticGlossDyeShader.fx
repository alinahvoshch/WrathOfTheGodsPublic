sampler baseTexture : register(s0);

float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float uDirection;
float2 uWorldPosition;
float2 uTargetPosition;
float2 uImageSize0;
float2 uImageSize1;
float2 uLegacyArmorSheetSize;
float3 uLightSource;
float3 uColor;
float3 uSecondaryColor;
float4 uLegacyArmorSourceRect;
float4 uSourceRect;

// Corresponds to Clip Studio Pant's Add (Glow) blending function.
float AddGlowBlend(float a, float b)
{
    return min(1, a + b);
}

float3 AddGlowBlend(float3 a, float3 b)
{
    return float3(AddGlowBlend(a.r, b.r), AddGlowBlend(a.g, b.g), AddGlowBlend(a.b, b.b));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate the 0-1 coords value relative to whatever frame in the texture is being used.
    float2 framedCoords = (coords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw;
    
    // Get the pixel's color on the base texture.
    float4 color = tex2D(baseTexture, coords);
    
    float brightness = dot(color.rgb, float3(0.3, 0.59, 0.11));
    
    // Make colors to the right pink/red, make colors to the left cyan.
    float redInterpolant = smoothstep(0.6, 0.4, framedCoords.x);    
    if (uDirection == -1)
        redInterpolant = 1 - redInterpolant;
    
    // Calculate the blend color based on the aforementioned positional interpolant, the saturation value, and the brightness of the original pixel.
    float3 blend = lerp(float3(0, 0.56, 1), float3(1, 0, 0.74), redInterpolant) * 0.6 + brightness - 0.5;
    float4 blendedColor = float4(AddGlowBlend(blend, color.rgb), 1) * color.a;
    
    // Apply darkening based on the luminosity of the sample color, to ensure that lighting is accounted for.
    float sampleColorLumoniosity = dot(sampleColor.rgb, float3(0.3, 0.6, 0.1));
    blendedColor.rgb *= sampleColorLumoniosity;
    
    return blendedColor * sampleColor.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}