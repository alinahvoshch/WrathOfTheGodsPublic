sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);
sampler godRayNoiseTexture : register(s2);
sampler rayParticleTexture : register(s3);

float globalTime;
float lightAngle;
float startingY;
float endingY;
float screenWidth;
float rayColorExponent;
float3 weakRayColor;
float3 strongRayColor;
float3 rayColorAdditiveBias;
float2 screenPosition;
float2 lightDirection;

float2 RotatedBy(float2 v, float angle)
{
    float c = cos(angle);
    float s = sin(angle);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float CalculateRayBrightness(float2 coords, float offsetAngle)
{
    // Rotate coordinate in accordance with the light angle.
    coords = RotatedBy(coords - 0.5, offsetAngle) + 0.5;
    
    float rayNoiseA = tex2D(godRayNoiseTexture, coords * float2(1.1, 0.02) * 1.1 + float2(globalTime * 0.06, 0));
    float rayNoiseB = tex2D(godRayNoiseTexture, coords * float2(2.4, 0.02) * 0.8 + float2(globalTime * -0.03, 0));
    
    return pow(saturate((rayNoiseA + rayNoiseB) * 0.5), rayColorExponent);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 localScreenPosition = position.xy + screenPosition.xy;
    
    // Combine noise to calculate the glow color.
    // This exists at the top of the god ray effect.
    float noiseA = tex2D(noiseTexture, position.xy / 1600 + float2(0.4, globalTime * -0.04));
    float noiseB = tex2D(noiseTexture, position.xy / 2300 + globalTime * 0.02);
    float lineOffset = lerp(noiseA * noiseB, 1, 0.7) * 250;
    float opacity = smoothstep(endingY, startingY, localScreenPosition.y + lineOffset) * 0.5;
    float4 glowColor = (tex2D(baseTexture, coords) * sampleColor - float4(0, 0, noiseA * 0.1, 0)) * opacity;
    
    // Calculate the color of the god rays.
    float rayOpacity = smoothstep(endingY, startingY, localScreenPosition.y + lineOffset - 100);
    float rayBrightness = CalculateRayBrightness(localScreenPosition / screenWidth, lightAngle) * rayOpacity;
    float4 rayColor = float4(lerp(weakRayColor, strongRayColor, rayBrightness), 0) * rayBrightness;
    rayColor += smoothstep(0.25, 0, rayBrightness) * float4(rayColorAdditiveBias, 0) * rayColor.a;
    
    // Calculate the upper glow.
    // This biases the colors towards pure white, so that when the player is at the top of the world the entire screen is prepared
    // for the subworld transition animation.
    float upperGlow = smoothstep(2300, 1800, localScreenPosition.y);
    
    return glowColor + upperGlow + rayColor;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}