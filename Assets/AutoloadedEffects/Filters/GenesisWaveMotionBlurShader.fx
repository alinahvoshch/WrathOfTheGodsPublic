sampler screenTexture : register(s0);
sampler tileTargetTexture : register(s1);

bool blurEnabled;
float blurIntensity;
float glowIntensity;
float2 blurOrigin;
float4 glowColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = 0;
    for (int i = 0; i < 40; i++)
    {
        float2 localCoords = (coords - blurOrigin) * (1 + i * blurIntensity * blurEnabled * 0.005) + blurOrigin;
        baseColor += tex2D(screenTexture, localCoords) * 0.025;
    }
    
    float depthInterpolant = 0;
    for (i = 0; i < 20; i++)
        depthInterpolant += any(tex2D(tileTargetTexture, coords + float2(0, i * 0.0035))) * 0.05;
    
    float distanceFromCenter = distance(coords, blurOrigin);
    float depthFadeoff = lerp(1, 0.3, depthInterpolant);
    depthFadeoff = lerp(depthFadeoff, 1, smoothstep(-0.2, 0.1, blurOrigin.y - coords.y));
    
    float blueInterpolant = smoothstep(0.1, 0.26, distanceFromCenter);
    float4 localGlowColor = glowColor - float4(blueInterpolant, blueInterpolant, 0, 0) * 0.3;
    baseColor += blurIntensity * glowIntensity * localGlowColor / pow(distance(blurOrigin, coords), 0.9) * depthFadeoff * 1.4;
    
    return baseColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
