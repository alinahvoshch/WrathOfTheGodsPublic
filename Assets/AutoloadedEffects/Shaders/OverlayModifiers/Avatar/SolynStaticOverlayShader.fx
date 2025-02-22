sampler2D baseTexture : register(s0);
sampler2D staticTexture : register(s1);
sampler2D dissolveTexture : register(s2);

float globalTime;
float dissolveInterpolant;
float2 imageSize;
float4 sourceRectangle;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 framedCoords = (coords * imageSize - sourceRectangle.xy) / sourceRectangle.zw;
    
    clip(tex2D(dissolveTexture, framedCoords * 1.4) - dissolveInterpolant);
    
    float4 baseColor = tex2D(baseTexture, coords);
    float4 staticColor = tex2D(staticTexture, framedCoords * 0.04) * baseColor.a;
    return lerp(baseColor, staticColor, distance(staticColor.r, 0.5) * 1.5 + dissolveInterpolant) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}