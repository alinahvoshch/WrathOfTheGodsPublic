sampler baseTexture : register(s0);

bool outlineOnly;
float globalTime;
float2 imageSize;
float4 sourceRectangle;

float OverlayBlend(float a, float b)
{
    if (a < 0.5)
        return a * b * 2;
    
    return 1 - (1 - a) * (1 - b) * 2;
}

float3 OverlayBlend(float3 a, float3 b)
{
    return float3(OverlayBlend(a.r, b.r), OverlayBlend(a.g, b.g), OverlayBlend(a.b, b.b));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 framedCoords = (coords * imageSize - sourceRectangle.xy) / sourceRectangle.zw;
    float4 baseColor = tex2D(baseTexture, coords);
    float4 color = float4(OverlayBlend(outlineOnly ? 1 : baseColor.rgb, sampleColor.rgb), 1) * sampleColor.a * baseColor.a;
    color.rg += color.a * 0.125;
    
    return color * pow(smoothstep(1, 0.6, framedCoords.y), 1.2);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}