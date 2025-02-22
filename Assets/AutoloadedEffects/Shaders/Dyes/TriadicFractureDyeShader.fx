sampler baseTexture : register(s0);

float swapHarshness;
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

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 framedCoords = (coords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw;
    float2 splitOffset = (sin(uTime * -3.2 + framedCoords.y * 2) * 0.5 + 0.5) * 5 / uImageSize0;
    
    float2 pixelationFactor = 1 / uImageSize0;
    splitOffset = floor(splitOffset / pixelationFactor) * pixelationFactor;
    
    float r = (tex2D(baseTexture, coords + float2(-0.707, 0) * splitOffset)).r;
    float g = (tex2D(baseTexture, coords + float2(0.707, 0) * splitOffset)).g;
    float b = (tex2D(baseTexture, coords + float2(0, 1) * splitOffset)).b;
    float a = tex2D(baseTexture, coords).a;
    
    return float4(r, g, b, a) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}