sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float distortOffsetFactor;
float distortCoordOffset;
float dissolveIntensity;
float2 imageSize;
float4 frame;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 framedCoords = (coords * imageSize - frame.xy) / frame.zw;
    
    float2 distortNoiseCoords = coords * 3.75 + distortCoordOffset;
    float2 distortionOffsetFactor = float2(0.06, 0.04) * pow(dissolveIntensity, 0.67) * distortOffsetFactor;
    
    float distanceFromHorizontalCenter = distance(framedCoords.x, 0.5);
    float distanceFromVerticalCenter = distance(framedCoords.y, 0.5);
    distortionOffsetFactor *= smoothstep(0.4, 0.3, distanceFromHorizontalCenter) * smoothstep(0.4, 0.3, distanceFromVerticalCenter);
    
    float distort = (float2(tex2D(noiseTexture, distortNoiseCoords).r, tex2D(noiseTexture, distortNoiseCoords + 0.4).r) - 0.5) * distortionOffsetFactor;
    
    float dissolveThreshold = dissolveIntensity + (1 - framedCoords.y) * 0.3;
    float dissolveOpacity = smoothstep(-0.04, 0, tex2D(noiseTexture, coords * 3 + distortCoordOffset * 0.75) - dissolveThreshold);
    
    sampleColor.rgb *= lerp(0.7, 1.3, tex2D(noiseTexture, framedCoords * 1.5));

    return tex2D(baseTexture, coords + distort) * sampleColor * dissolveOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}