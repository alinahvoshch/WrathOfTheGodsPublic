texture baseTexture;
sampler2D baseTextureSampler = sampler_state
{
    texture = <baseTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float globalTime;
float blurWeights[7];
float2 blurOffset;
float3 additiveColor;
float3 biasRedTowards;

float4 CalculateBlurColor(float2 coords)
{
    float4 blurColor = 0;
    for (int i = -3; i < 4; i++)
    {
        for (int j = -3; j < 4; j++)
        {
            float weight = blurWeights[abs(i) + abs(j)];
            blurColor += tex2D(baseTextureSampler, coords + float2(i, j) * blurOffset) * weight;
        }
    }
    
    return blurColor;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = CalculateBlurColor(coords) * sampleColor;
    
    // Calculate saturation in relation to luminosity. The more biased colors are away from the greyscale equivalent of the color, the more saturated it's
    // considered for the calculations below.
    float luminosity = dot(color.rgb, float3(0.3, 0.6, 0.1));
    float saturation = saturate(distance(color.r, luminosity) + distance(color.g, luminosity) + distance(color.b, luminosity));
    
    // Determine how close the color already is to blue, and use it along with the saturation to figure out how much additive colorations should be done for this pixel.
    // This is heavily discounted for red colors, but the for the sake of allowing cyans it does not account for greens.
    float alreadyBlueInterpolant = saturate(dot(color.rgb, float3(-2, 0, 1)));
    color += float4(additiveColor, 0) * pow(saturation, 2) * (1 - alreadyBlueInterpolant) * 1.8;
    
    // Bias sharp red colors towards a designated blue color.
    float sharpRedInterpolant = saturate(dot(color.rgb, float3(1, -1, -0.2)));
    color.rgb = lerp(color.rgb, biasRedTowards, smoothstep(0.7, 0.85, sharpRedInterpolant));
    
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}