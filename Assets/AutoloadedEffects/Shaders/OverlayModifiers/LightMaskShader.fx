sampler baseTexture : register(s0);
sampler lightTexture : register(s1);

float globalTime;
float2 zoom;
float2 screenSize;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float4 color = tex2D(baseTexture, coords);
    color.rgb *= tex2D(lightTexture, (position.xy / screenSize - 0.5) / zoom + 0.5);
    
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}