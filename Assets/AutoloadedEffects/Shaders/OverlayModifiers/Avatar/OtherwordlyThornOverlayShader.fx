sampler2D baseTexture : register(s0);

float2 screenSize;

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
    // Used to negate the need for an inverted if (baseColor.a > 0) by ensuring all of the edge checks fail.
    float4 baseColor = tex2D(baseTexture, coords);
    float alphaOffset = baseColor.a <= 0.7;
    
    // Check if there are any empty pixels nearby. If there are, that means this pixel is at an edge, and should be colored accordingly.
    float left = tex2D(baseTexture, convertFromScreenCoords(convertToScreenCoords(coords) + float2(-2, 0))).a + alphaOffset;
    float right = tex2D(baseTexture, convertFromScreenCoords(convertToScreenCoords(coords) + float2(2, 0))).a + alphaOffset;
    float top = tex2D(baseTexture, convertFromScreenCoords(convertToScreenCoords(coords) + float2(0, -2))).a + alphaOffset;
    float bottom = tex2D(baseTexture, convertFromScreenCoords(convertToScreenCoords(coords) + float2(0, 2))).a + alphaOffset;
    
    // Use step instead of branching in order to determine whether neighboring pixels are invisible.
    float leftHasNoAlpha = step(left, 0);
    float rightHasNoAlpha = step(right, 0);
    float topHasNoAlpha = step(top, 0);
    float bottomHasNoAlpha = step(bottom, 0);
    
    // Use addition instead of the OR boolean operator to get a 0-1 value for whether an edge is invisible.
    // The equivalent for AND would be multiplication.
    float conditionOpacityFactor = 1 - saturate(leftHasNoAlpha + rightHasNoAlpha + topHasNoAlpha + bottomHasNoAlpha);
    
    return baseColor + (1 - conditionOpacityFactor) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}