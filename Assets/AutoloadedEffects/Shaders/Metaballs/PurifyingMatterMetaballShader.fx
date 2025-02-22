sampler metaballContents : register(s0);
sampler overlayTexture : register(s1);

float blurWeights[9];
float blurOffset;
float2 screenSize;
float2 layerSize;
float2 layerOffset;
float4 edgeColor;
float2 singleFrameScreenOffset;

float4 GaussianBlur(float2 coords, float offsetFactor)
{
    float4 result = 0;
    for (int i = -4; i < 5; i++)
    {
        for (int j = -4; j < 5; j++)
        {
            float weight = blurWeights[abs(i) + abs(j)];
            result += any(tex2D(metaballContents, coords + float2(i, j) * blurOffset * offsetFactor)) * weight;
        }
    }
    
    return result;
}

// The usage of these two methods seemingly prevents imprecision problems for some reason.
float2 convertToScreenCoords(float2 coords)
{
    return coords * screenSize;
}

float2 convertFromScreenCoords(float2 coords)
{
    return coords / screenSize;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate the base color. This is the calculated from the raw objects in the metaball render target.
    float4 baseColor = tex2D(metaballContents, coords);
    
    // Calculate layer colors.
    float4 layerColor = tex2D(overlayTexture, (coords + layerOffset + singleFrameScreenOffset) * screenSize / layerSize);
    float4 defaultColor = layerColor * tex2D(metaballContents, coords) * sampleColor;
    
    float4 blurColor = smoothstep(0, 0.9, GaussianBlur(coords, 1));
    float4 glowColor = smoothstep(0, 0.9, GaussianBlur(coords, 0.2));
    glowColor = lerp(glowColor, glowColor * edgeColor, 0.5) * 0.5;
    
    return defaultColor + (blurColor * edgeColor + glowColor) * (1 - any(defaultColor));
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}