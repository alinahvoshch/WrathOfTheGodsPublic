sampler baseTexture : register(s0);

float blurOffset;
float blurWeights[9];
float avoidanceOpacityInterpolant;
float bottomOpacityBias;
float2 avoidanceDirection;

float4 CalculateBlurColor(float2 coords)
{
    float4 baseColor = tex2D(baseTexture, coords);
    
    float4 blurColor = 0;
    for (int i = -2; i < 3; i++)
    {
        for (int j = -2; j < 3; j++)
        {
            float weight = blurWeights[abs(i) + abs(j)];
            float4 localColor = tex2D(baseTexture, coords + float2(i, j) * blurOffset);
            float temperature = localColor.r + localColor.g - localColor.b * 3;
            blurColor += float4(1, 0.06, 0, 0) * weight * saturate(temperature);
        }
    }
    
    return blurColor;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float bottomOpacity = smoothstep(0.65, 0.55, coords.y - bottomOpacityBias);
    float2 avoidancePosition = 0.5 + avoidanceDirection * 0.2;
    float distanceToAvoidancePosition = distance(coords, avoidancePosition);    
    float avoidanceOpacity = smoothstep(0.085, 0.26, distance(coords, avoidancePosition));
    avoidanceOpacity = lerp(1, avoidanceOpacity, avoidanceOpacityInterpolant);
    
    return CalculateBlurColor(coords) * sampleColor * bottomOpacity * avoidanceOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}