sampler screenTexture : register(s0);
sampler fogTexture : register(s1);

float globalTime;
float intensity;
float fogDensityExponent;
float2 screenPosition;
float2 screenSize;
float4 fogColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Apply averaged FBM steps to determine an overall fog density value.
    // Each step's previous value affects the coordinates of the next, resulting in a complex, cloudy layering effect.
    float fogDensity = 0;
    float2 screenOffset = screenPosition / screenSize * 0.05 + float2(globalTime * 0.15, sin(coords.x * 6.283 + globalTime * 2) * 0.05);
    for (int i = 0; i < 5; i++)
    {
        float2 screenOffsetCoords = coords * (i * 0.1 + 0.2);
        float2 fogCoords = screenOffsetCoords * float2(0.45, 1) * (1 + i * 0.76) + fogDensity * 0.07 + (screenOffset * (i == 4));
        
        float localFogDensity = tex2D(fogTexture, fogCoords) + tex2D(fogTexture, fogCoords * 3) * 0.125;
        fogDensity += localFogDensity;
    }
    
    // Average the aforementioned results and apply a strong exponential effect.
    fogDensity = pow(fogDensity * 0.2, fogDensityExponent) * (1 + smoothstep(0.16, 0.01, distance(coords, 0.5)) * 1.1);
    
    // Additively apply the fog color.
    float4 baseColor = lerp(tex2D(screenTexture, coords), fogColor, intensity * fogDensity);
    return baseColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
