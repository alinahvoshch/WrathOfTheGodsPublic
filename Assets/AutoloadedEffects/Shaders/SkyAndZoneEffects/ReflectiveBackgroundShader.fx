sampler baseTexture : register(s0);
sampler screenTexture : register(s1);

float globalTime;

float4 InfiniteMirrorEffect(float maxIterations, float2 coords)
{
    float breakOutIteration = maxIterations;
    for (int i = 0; i < maxIterations; i++)
    {
        if (coords.x < 0.05 || coords.x > 0.95 || coords.y < 0.05 || coords.y > 0.95)
        {
            breakOutIteration = i;
            break;
        }

        coords = (coords - 0.5) * 1.34 + 0.5;
        coords.x += sin(i * 0.4 + globalTime) * 0.003;
    }
    
    return tex2D(screenTexture, coords) * (1 - breakOutIteration / maxIterations);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float innerColorInterpolant = 1;    
    float4 innerColor = InfiniteMirrorEffect(15, coords);
    return float4(0, 0, 0, 1) + innerColor;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}