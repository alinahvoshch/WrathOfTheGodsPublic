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

float goldHairGradientCount;
float3 goldHairGradient[10];

float redHairGradientCount;
float3 redHairGradient[10];

float3 PaletteLerp(float interpolant, float3 gradient[10], float gradientCount)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = clamp(startIndex + 1, 0, gradientCount - 1);
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 pixelationFactor = 2 / uImageSize0;
    float2 pixelatedCoords = floor(coords / pixelationFactor) * pixelationFactor;
    
    // Calculate the 0-1 coords value relative to whatever frame in the texture is being used.
    // To ensure that calculations that interpolate according to this don't become unnatural gradients, this uses the pixelated coordinates.
    float2 framedCoords = (pixelatedCoords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw;
    
    float4 color = tex2D(baseTexture, coords);
    color = float4(lerp(color.rgb, sampleColor.rgb, 0.05), 1) * color.a * sampleColor.a;
    
    float luminosity = smoothstep(0.15, 0.95, dot(color.rgb, float3(0.3, 0.6, 0.1))) * 0.99;
    
    float3 goldHairColor = PaletteLerp(luminosity, goldHairGradient, goldHairGradientCount);
    float3 redHairColor = PaletteLerp(luminosity * 0.8, redHairGradient, redHairGradientCount);
    
    // Bias towards red hair the higher up the position is, similar to Solyn's hair.
    float hairYInterpolant = smoothstep(0.16, 0.2, framedCoords.y);
    float redHairBias = pow(1 - hairYInterpolant, 4.5) * smoothstep(0.7, 0.5, framedCoords.x);
    float3 hairColor = lerp(goldHairColor, redHairColor, redHairBias);
    
    return float4(hairColor, 1) * color.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}