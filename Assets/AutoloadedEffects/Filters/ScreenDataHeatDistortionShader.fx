sampler screenTexture : register(s0);
sampler distortionTexture : register(s1);
sampler distortionExclusionTexture : register(s2);
sampler noiseTexture : register(s3);
sampler distortionMapTexture : register(s4);

float globalTime;
float maxDistortionOffset;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Load distortion (exclusion data.
    float4 exclusionData = tex2D(distortionExclusionTexture, coords);
    float exclusionIntensity = sqrt(max(exclusionData.a, dot(exclusionData.rgb, 0.333)));
    float4 distortionData = tex2D(distortionTexture, coords) * (1 - exclusionIntensity);
    float distortionIntensity = maxDistortionOffset * length(distortionData);
    
    // Apply distortion effects.
    float distortionAngle = tex2D(noiseTexture, coords * 2 + float2(globalTime * 0.06, 0)) * 20 + globalTime * 3;
    float2 distortionDirection = float2(cos(distortionAngle), sin(distortionAngle));    
    coords += distortionDirection * distortionIntensity * 0.002;
    
    return tex2D(screenTexture, coords);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}