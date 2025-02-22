sampler baseTexture : register(s0);

float globalTime;
float bottomThreshold;
float2 screenPosition;
float4 gradientTop;
float4 gradientBottom;
float4 additiveTopOfWorldColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 localScreenPosition = screenPosition + position.xy;
    
    float4 gradient = lerp(gradientTop, gradientBottom, smoothstep(0, bottomThreshold, coords.y));
    float4 color = tex2D(baseTexture, coords) * sampleColor * gradient;
    
    color += additiveTopOfWorldColor * smoothstep(4400, 1600, localScreenPosition.y);
    
    return color;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}