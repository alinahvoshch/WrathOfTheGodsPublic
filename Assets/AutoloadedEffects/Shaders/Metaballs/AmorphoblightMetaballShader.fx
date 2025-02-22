sampler metaballContents : register(s0);
sampler distortionTexture : register(s1);
sampler stripeGlowTextureA : register(s2);
sampler stripeGlowTextureB : register(s3);

float globalTime;
float zoom;
float2 screenSize;
float2 layerSize;
float2 layerOffset;
float2 singleFrameScreenOffset;
float4 edgeColor;
float4 baseInnerColor;

float2 CalculateWarpDirection(float2 worldUV, float timeFactor)
{
    float angle = tex2D(distortionTexture, worldUV * 0.4 + float2(0.9, 0.6) * timeFactor * globalTime) * 9;
    float2 warpDirection = float2(cos(angle), sin(angle));
    return warpDirection;
}

float AperiodicSin(float x)
{
    return (cos(x * 3.141) + sin(x * 2.718)) * 0.5;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 pixelationFactor = 5 / screenSize;
    float2 worldUV = (coords + layerOffset + singleFrameScreenOffset) * screenSize / layerSize;
    worldUV = floor(worldUV / pixelationFactor) * pixelationFactor;
    
    float4 undistortedMetaballData = tex2D(metaballContents, coords);
    
    // Apply warping to the coordinates.
    coords += CalculateWarpDirection(worldUV, 0.072) * 3 / screenSize;
    
    // Calculate coordinates for the inner glow noise relative to the world position.
    float2 innerColorNoiseCoords = (worldUV + float2(0, globalTime * 0.004)) * zoom;
    innerColorNoiseCoords = innerColorNoiseCoords + CalculateWarpDirection(worldUV * 2, -0.06) * 5 / screenSize;
    
    // Calculate the inner glow noise and apply the glow/redshift effect in accordance with it.
    float threshold = AperiodicSin(globalTime * 0.07 + worldUV.x * -0.2);
    float innerGlowNoise = sqrt(tex2D(stripeGlowTextureA, innerColorNoiseCoords) * tex2D(stripeGlowTextureB, innerColorNoiseCoords * 2));
    float innerGlowInterpolant = abs(threshold - innerGlowNoise) * 2;
    innerGlowInterpolant = cos(innerGlowInterpolant * 10 - globalTime * 0.1 + distance(worldUV, 0.5) * 10) * 0.5 + 0.5;
    
    float4 innerColor = baseInnerColor * lerp(1, 2, innerGlowInterpolant);
    innerColor.r += smoothstep(0.2, 0, distance(innerGlowInterpolant, 0.4)) * 0.8;
    
    // Combine everything together.
    float4 metaballData = tex2D(metaballContents, coords);    
    float4 color = lerp(innerColor, edgeColor, metaballData.g) * saturate(metaballData.a + undistortedMetaballData.a * undistortedMetaballData.r);
    return color;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}