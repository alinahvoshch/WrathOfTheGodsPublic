sampler2D baseTexture : register(s0);

bool blurAtCenter;
bool performPositionCutoff;
float globalTime;
float blurOffset;
float blurWeights[7];
float2 cutoffOrigin;
float2 forwardDirection;

float4 CalculateBlurColor(float2 coords)
{
    float4 blurColor = 0;
    for (int i = -3; i < 4; i++)
    {
        for (int j = -3; j < 4; j++)
        {
            float weight = blurWeights[abs(i) + abs(j)];
            blurColor += any(tex2D(baseTexture, coords + float2(i, j) * blurOffset)) * weight;
        }
    }
    
    return blurColor;
}

float4 PixelShaderFunction(float4 position : SV_POSITION, float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(baseTexture, coords) * float4(0, 0, 0, 1);    
    float blur = CalculateBlurColor(coords) * (1 - color.a);
    float centerFade = lerp(1, smoothstep(0.04, 0.054, distance(coords, 0.5)), blurAtCenter);
    
    bool behindCutoffOrigin = dot(position.xy - cutoffOrigin, forwardDirection) <= 0;
    bool cutOffPixel = performPositionCutoff * behindCutoffOrigin;
    
    return (color + sampleColor * blur) * centerFade * (1 - cutOffPixel);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}