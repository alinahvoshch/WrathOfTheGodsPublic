sampler baseTexture : register(s0);
sampler crackNoiseTexture : register(s1);
sampler fadeOutNoiseTexture : register(s2);
sampler codeTexture : register(s3);

float barOffsetMax;
float intensity;
float codeAppearanceInterpolant;
float negativeZoneRadius;
float innerGlowRadius;
float fadeOut;
float crackBaseRadius;
float redCodeInterpolant;
float2 impactPosition;
float2 screenSize;
float2 screenPosition;
float2 zoom;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Calculate the base color.
    float4 color = tex2D(baseTexture, coords);
    
    // Calculate the distance from the impact point, along with polar coordinates, for later.
    float distanceFromImpact = distance(position.xy, impactPosition) / zoom.x;
    float2 polar = float2(atan2(position.y - impactPosition.y, position.x - impactPosition.x) / 6.283 + 0.5, distanceFromImpact / screenSize.x);
    
    // Calculate the effective distance as used for crack calculations.
    // This uses the distance from the impact as a baseline, but also uses two other things, for the following outcomes and reasons:
    // 1. A polar noise sample. This helps ensure that the cracks don't have a noticeably radial edge, and can fade out a bit earlier than usual.
    // 2. The effect's intensity. This ensures that the cracks spread out over time, rather than appearing instantly.
    float crackDistance = distanceFromImpact / crackBaseRadius + tex2D(fadeOutNoiseTexture, polar * 4) * 0.8 + pow(1 - intensity, 4) * 1.8;
    
    // Calculate the glow of cracks.
    float crackNoise = tex2D(crackNoiseTexture, polar * float2(7, 2)) * tex2D(crackNoiseTexture, polar * float2(2, 1));
    float crackGlow = smoothstep(1.8, 1, crackDistance) / pow(crackNoise, 3) * 0.1;
    crackGlow *= 0.2 + tex2D(fadeOutNoiseTexture, polar * 4);
    crackGlow = 1 - exp(crackGlow * -2);
    crackGlow = smoothstep(0, 1, crackGlow) * pow(1 - fadeOut, 3);
    
    color += crackGlow;
    
    // Calculate an inner glow, to give a definitive origin to the cracks.
    // This uses noise based on the effective polar angle of the coords, to make the crack center feel less geometrically perfect/circular and more like a crack one might expect in glass.
    float innerGlowDistance = distanceFromImpact / innerGlowRadius - tex2D(fadeOutNoiseTexture, polar.x + 0.3) * 200 / innerGlowRadius;
    float innerGlow = smoothstep(1, 0, innerGlowDistance);
    color += innerGlow * intensity * (1 - fadeOut) * 1.1;
    
    // Make colors negative within a certain radius.
    float negativeInterpolant = smoothstep(500, 0, distanceFromImpact - negativeZoneRadius) * (1 - fadeOut);
    color.rgb = lerp(color.rgb, 1 - color.rgb, negativeInterpolant);
    
    // Replace dark colors with an appearance of the code background.
    float4 codeColor = tex2D(codeTexture, coords + screenPosition / screenSize);
    float4 redCodeColor = float4(codeColor.g * 1.54, codeColor.r, codeColor.b * 0.25, codeColor.a);
    codeColor = lerp(codeColor, redCodeColor, redCodeInterpolant);
    
    float brightness = dot(color.rgb, float3(0.3, 0.6, 0.1));
    float codeInterpolant = smoothstep(0.11, 0.05, brightness) * codeAppearanceInterpolant * pow(1 - fadeOut, 2.5);
    color = lerp(color, codeColor, codeInterpolant);
    
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}