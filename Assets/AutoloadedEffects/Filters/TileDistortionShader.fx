sampler screenTexture : register(s0);
sampler distortionContentsTexture : register(s1);
sampler tileContentsTexture : register(s2);

float maxDistortionOffset;
float2x2 projection;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 zoomedCoords = mul(coords - 0.5, projection) + 0.5;    
    float4 distortionData = tex2D(distortionContentsTexture, zoomedCoords);
    
    float2 distortionOffset = distortionData.r * float2(0, maxDistortionOffset);
    float4 distortedColor = tex2D(screenTexture, coords + distortionOffset);
    
    bool anyTiles = any(tex2D(tileContentsTexture, zoomedCoords + distortionOffset));
    
    float4 baseColor = tex2D(screenTexture, coords);
    return lerp(baseColor, distortedColor, anyTiles);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
