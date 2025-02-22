sampler baseTexture : register(s0);
sampler fogTexture : register(s1);

float globalTime;
float radius;
float2 smokeCenter;
float2 zoom;
float2 screenSize;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 offsetFromCenter = (position.xy - smokeCenter) / zoom.x;
    float distanceFromCenter = length(offsetFromCenter);
    float distancePastEdge = distanceFromCenter - radius;
    float radiusInterpolant = radius / 3000;
    
    float2 polar = float2(atan2(offsetFromCenter.y, offsetFromCenter.x) / 6.283 + 0.5, distanceFromCenter / screenSize.x);
    
    // Use FBM for wispy noise values.
    float smokeWispOffsetInterpolant = 0;
    for (int i = 0; i < 5; i++)
        smokeWispOffsetInterpolant = tex2D(fogTexture, polar * float2(15, i * 0.6 + 0.2) + smokeWispOffsetInterpolant * 0.09 + globalTime * float2(0, 0.05));
    
    // Start with the base color.
    float4 color = tex2D(baseTexture, coords);
    
    // Apply smoke darkening effects based on noise and distance.
    float smokeBrightness = lerp(0.01, 0.04, tex2D(fogTexture, polar * float2(14, 3) + float2(smokeWispOffsetInterpolant * -0.3, globalTime * 0.08)));
    smokeBrightness *= smoothstep(1200, 400, distancePastEdge);
    float smokeInterpolant = smoothstep(0, 250, distancePastEdge + smokeWispOffsetInterpolant * radiusInterpolant * 500);
    
    float4 smokeColor = smoothstep(0, 0.5, smokeInterpolant) * smokeBrightness;    
    color = lerp(color, smokeColor, smokeInterpolant);
    
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
