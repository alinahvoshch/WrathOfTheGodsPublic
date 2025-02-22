sampler staticTexture : register(s0);
sampler windTexture : register(s1);
sampler frostTexture : register(s2);
sampler frostTurbulenceTexture : register(s3);

float time;
float vignetteSwirlTime;
float vignetteCurvature;
float vignetteAppearanceBoost;
float2 playerUV;
float2 textureSize;
float3 windColorA;
float3 windColorB;
float3 frostVignetteColorA;
float3 frostVignetteColorB;

float2 AspectRatioAdjust(float2 coords)
{
    return (coords - 0.5) * float2(1, textureSize.y / textureSize.x) + 0.5;
}

float CalculateStaticIntensity(float2 coords)
{
    // Calculaate noise static.
    float noise = frac(tex2D(staticTexture, frac(coords * 16)).r + time * 0.16);
    
    // Use the static for a nother static sampling, resulting in a near completely random visual.
    noise = tex2D(staticTexture, frac(noise * 2));
    
    // Calculate how brightness the static is, based on how far the pixel is from the player.
    float distanceFromPlayer = distance(AspectRatioAdjust(playerUV), AspectRatioAdjust(coords));
    float staticBrightness = smoothstep(0.15, 0, distanceFromPlayer) * 0.075 + smoothstep(0.035, 0.013, distanceFromPlayer) * 0.2;
    
    return pow(noise, 1.5) * (0.1 - staticBrightness);
}

float SignedPow(float x, float n)
{
    return pow(abs(x), n) * sign(x);
}

float4 CalculateWindColor(float2 coords)
{
    float2 windCoords = frac(coords);
    float windDirection = SignedPow(coords.y - 0.5, 0.6);
    
    // Apply curvature distortion at the top and bottom of the texture.
    windCoords.y -= pow(abs(windCoords.x - 0.5), 2) * windDirection * 1.5;
    
    // Apply scroll effects.
    windCoords.x -= time * 0.46;
    
    // Accent the noise a bit.
    windCoords.y += tex2D(windTexture, windCoords * -3 + time * 0.54) * 0.01;
    windCoords.y += sin(windCoords.x * 6 + time * 5) * 0.02;
    
    // Calculate colors.
    float distortionNoise = 1.04 - tex2D(windTexture, windCoords * float2(0.5, 3.5) + float2(time * -0.1, 0));
    float colorNoise = smoothstep(1.2, -0.2, tex2D(windTexture, windCoords * float2(0.6, 2) + distortionNoise)) * distortionNoise;
    float brightness = 0.01 / colorNoise;
    return lerp(float4(windColorA, 0), float4(windColorB, 0), cos(colorNoise * 7 + time) * 0.5 + 0.5) * brightness;
}

float CalculateFrostVignetteColorInterpolant(float2 coords)
{
    float2 curve = pow(abs(coords * 2 - 1), 1 / vignetteCurvature);
    float edge = pow(length(curve), vignetteCurvature);
    
    return smoothstep(1, 1.4, edge + tex2D(frostTexture, coords + float2(time * 0.4, 0)) * 0.2 + vignetteAppearanceBoost) * 0.43;
}

float4 CalculateFrostVignetteColor(float2 coords, float4 baseColor)
{
    float2 polar = float2(distance(coords, 0.5), atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5);
    polar.x -= time * 0.5;
    polar.y -= vignetteSwirlTime * 0.15;
    
    float2 frostCoords = polar * 3;
    
    float turbulence = tex2D(frostTurbulenceTexture, coords + vignetteSwirlTime * 0.2) * 0.12;
    float frostTexturing = tex2D(frostTexture, frostCoords + turbulence);
    float vignetteInterpolant = CalculateFrostVignetteColorInterpolant(coords);
    float3 frostColor = lerp(frostVignetteColorA, frostVignetteColorB, sin(frostTexturing * 20) * 0.5 + 0.5);
    
    return lerp(baseColor, float4(frostColor, 1), vignetteInterpolant);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = float4(0, 0, 0, 1) + CalculateWindColor(coords) + CalculateStaticIntensity(coords);
    color = CalculateFrostVignetteColor(coords, color);
    
    return color;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}