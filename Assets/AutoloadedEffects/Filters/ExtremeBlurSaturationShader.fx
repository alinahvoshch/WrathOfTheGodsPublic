sampler screenTexture : register(s0);

float greyscaleInterpolant;
float globalTime;
float blurIntensity;
float returnToNormalInterpolant;
float2 impactPoint;
float4x4 contrastMatrix;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(screenTexture, coords);
    
    // We do a bit of fucking BLUR!!!
    float4 blurredColor = 0;
    float2 directionFromImpact = normalize(impactPoint - coords);
    float2 blurOffset = directionFromImpact * blurIntensity * 0.0024;
    for (int i = 0; i < 70; i++)
        blurredColor += tex2D(screenTexture, coords + i * blurOffset);    
    blurredColor *= 0.014286;
    
    // Interpolate towards greyscale colors.
    float blurredGreyscale = pow(dot(blurredColor.rgb, float3(0.3, 0.6, 0.1)), 2.4);
    blurredColor = lerp(blurredColor, blurredGreyscale, greyscaleInterpolant);
    
    // Apply contrast visuals.
    // This incorporates a tiny bit of color variance so that the screen isn't restricted by the Avatar's cyans and reds when rendering.
    blurredColor = mul(blurredColor + float4(-0.01, 0.023, 0.011, 0), contrastMatrix);
    
    return lerp(blurredColor, baseColor, returnToNormalInterpolant);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
