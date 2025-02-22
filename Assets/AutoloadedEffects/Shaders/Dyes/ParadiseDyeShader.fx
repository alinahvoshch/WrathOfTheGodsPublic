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

float Hash12(float2 p)
{
    return frac(sin(dot(p, float2(67, 74))) * 3000);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 framedCoords = (coords * uImageSize0 - uSourceRect.xy) / uSourceRect.zw;
    float4 color = tex2D(baseTexture, coords) * sampleColor;

    float warp = Hash12(uTime);
    
    float2 pixelationFactor = 2 / uImageSize0;
    float2 pixelatedCoords = floor(coords / pixelationFactor) * pixelationFactor;
    float3 noise = Hash12(pixelatedCoords + warp) * smoothstep(0, 0.45, dot(color.rgb, 0.333));
    
    return color.a * float4(noise, 1);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}