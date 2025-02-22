sampler baseTexture : register(s0);

float globalTime;
float2 imageSize;
float4 sourceRectangle;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 framedCoords = (coords * imageSize - sourceRectangle.xy) / sourceRectangle.zw;
    float4 baseColor = tex2D(baseTexture, coords);
    
    float luminosity = dot(baseColor.rgb, float3(0.3, 0.6, 0.1));
    luminosity -= framedCoords.y * 0.6;
    
    float4 color = float4(luminosity, luminosity, luminosity, 1) * baseColor.a;
    return color * pow(smoothstep(1, 0.6, framedCoords.y), 1.2) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}