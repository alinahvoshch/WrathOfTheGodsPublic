sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float materializeInterpolant;
float fadeToWhite;
float2 baseTextureSize;

float2 Pixelate(float2 coords)
{
    float2 pixelationFactor = 2 / baseTextureSize;
    float2 pixelatedCoords = round(coords / pixelationFactor) * pixelationFactor;
    return pixelatedCoords;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 pixelatedCoords = Pixelate(coords);
    float2 polar = float2(atan2(0.5 - pixelatedCoords.y, 0.5 - pixelatedCoords.x) / 6.283 + 0.5, distance(pixelatedCoords, 0.5));
    polar.y *= 0.2;
    polar.x += polar.y * 2.2;
    
    float2 scatterNoiseCoords = polar * float2(1, 0.85);
    
    float2 scatteredCoords = float2(tex2D(noiseTexture, scatterNoiseCoords).r, tex2D(noiseTexture, scatterNoiseCoords + 0.32).r);
    scatteredCoords = coords + (scatteredCoords - 0.5) * 1.1;
    
    float localMaterializeInterpolant = saturate(materializeInterpolant + tex2D(noiseTexture, pixelatedCoords * 4 + 0.54) * 0.2);
    float2 warpedCoords = lerp(Pixelate(scatteredCoords), coords, localMaterializeInterpolant);
    
    float4 color = tex2D(baseTexture, warpedCoords);
    color = lerp(color, color.a, fadeToWhite);
    
    float outerFade = smoothstep(0.5, 0.43, distance(coords, 0.5));
    float edgeFade = smoothstep(0.3, 0.2, distance(coords.x, 0.5) - (1 - materializeInterpolant) * 0.2);
    return color * sampleColor * smoothstep(0, 0.5, localMaterializeInterpolant) * outerFade * edgeFade;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}