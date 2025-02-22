sampler baseTexture : register(s0);
sampler tileMaskTexture : register(s1);

float globalTime;
float opacity;
float2 screenSize;
float2 oldScreenPosition;
float2 screenPosition;

float Random(float2 coords)
{
    return abs(frac(sin(dot(coords, float2(17.8342, 74.8819) + globalTime * 0.1)) * 53648));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 pixelationFactor = 2 / screenSize;
    float2 pixelatedCoords = floor(coords / pixelationFactor) * pixelationFactor;
    
    float4 baseColor = tex2D(baseTexture, coords);
    float rng = Random(pixelatedCoords);
    
    float2 screenOffset = (screenPosition - oldScreenPosition) / screenSize * 1.4;
    return lerp(baseColor, rng, (tex2D(tileMaskTexture, coords + screenOffset).r >= 0.2) && opacity >= 0.99);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}