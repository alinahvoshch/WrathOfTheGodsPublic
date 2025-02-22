sampler screenTexture : register(s0);
sampler distortionTexture : register(s1);
sampler noiseTexture : register(s2);

float globalTime;
float maxDistortionOffset;
float2 screenPosition;
float2 screenSize;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float distortionAngle = tex2D(noiseTexture, (position.xy + screenPosition) * 0.0006) * 20 + globalTime * 4;
    float2 distortionDirection = float2(cos(distortionAngle), sin(distortionAngle) * 0.2);
    float4 distortionData = tex2D(distortionTexture, coords);
    
    float2 pixelationFactor = 2 / screenSize;
    float2 distortionOffset = distortionDirection * distortionData.r * maxDistortionOffset;
    float2 pixelatedCoords = coords + distortionOffset;
    pixelatedCoords = round(pixelatedCoords / pixelationFactor) * pixelationFactor;
    
    return tex2D(screenTexture, lerp(coords, pixelatedCoords, length(distortionOffset) > 0.0001));
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}