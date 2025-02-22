sampler baseTexture : register(s0);

float globalTime;
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
    
    float4 color = tex2D(baseTexture, coords) * sampleColor;
    float brightness = dot(color.rgb, float3(0.3, 0.6, 0.1));
    
    float angleFromCenter = atan2(framedCoords.y - 0.5, framedCoords.x - 0.5);
    float distanceFromCenter = distance(framedCoords, 0.5);
    float hue = cos(angleFromCenter + uTime * 0.43 + brightness * 5 - color.b * 5 - distanceFromCenter * 9) * 0.5 + 0.5;
    
    float darkeningFactor = pow(brightness + (1 - color.a), 0.45);
    float4 modifiedColor = lerp(float4(1, 0.6, 1, 1), float4(0.47, 1, 2, 1), hue);
    modifiedColor *= float4(darkeningFactor, darkeningFactor, darkeningFactor, 1);
    modifiedColor += smoothstep(0.7, 1, brightness) * 0.3;
    
    color = lerp(color, color.a * modifiedColor, 0.95);
    
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}