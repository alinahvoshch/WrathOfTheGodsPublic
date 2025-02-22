sampler baseTexture : register(s0);
sampler tileTargetTexture : register(s1);
sampler noiseTexture : register(s2);
sampler grassDataTexture : register(s3);
sampler vineNoiseTexture : register(s4);

float time;
float glowRadius;
float genesisIntensities[15];
float maxDistortionOffset;
float maxElectricityHeight;
float electricityCoverage;
float2 zoom;
float2 screenSize;
float2 screenPosition;
float2 lastScreenPosition;
float2 screenOffset;
float2 genesisPositions[15];
float4 magicColor;

float4 Sample(float2 coords)
{
    float4 tileTargetData = tex2D(tileTargetTexture, coords);
    return tileTargetData;
}

float2 ConvertToScreenCoords(float2 coords)
{
    return coords * screenSize;
}

float2 ConvertFromScreenCoords(float2 coords)
{
    return coords / screenSize;
}

bool AtEdge(float2 coords)
{
    float2 screenCoords = ConvertToScreenCoords(coords);
    float left = Sample(ConvertFromScreenCoords(screenCoords + float2(-2, 0))).a;
    float right = Sample(ConvertFromScreenCoords(screenCoords + float2(2, 0))).a;
    float top = Sample(ConvertFromScreenCoords(screenCoords + float2(0, -2))).a;
    float bottom = Sample(ConvertFromScreenCoords(screenCoords + float2(0, 2))).a;
    float4 color = Sample(coords);
    bool anyEmptyEdge = !any(left) || !any(right) || !any(top) || !any(bottom);
    
    return anyEmptyEdge && any(color.a);
}

float2 Pixelate(float2 coords)
{
    float2 pixelationFactor = 8 / screenSize;
    return floor(coords / pixelationFactor) * pixelationFactor;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Calculate world-relative position and UVs.
    float2 worldPosition = ((position.xy - screenSize * 0.5) / zoom + screenSize * 0.5 + screenPosition);
    float2 worldCoords = worldPosition / screenSize;
    
    // Calculate zoom-relative UVs and use them to sample tile data.
    float2 zoomedCoords = (coords - 0.5 + screenOffset) / zoom + 0.5;    
    float4 tileData = tex2D(tileTargetTexture, zoomedCoords);
    
    // Calculate a radial distortion offset for use for the magic effect.
    // This is used to make the warped edges have a sharper, more random look, almost like electricity.
    float distortionAngle = tex2D(noiseTexture, worldCoords * float2(3, 1) + float2(time * 0.02, 0)) * 20;
    float2 distortion = (float2(cos(distortionAngle), sin(distortionAngle)) * float2(0.4, 1) * maxDistortionOffset / screenSize);
    
    // Calculate an opacity value based on the distortion offset, that makes downward offsets fade out, making it look like the magic pulsation is strongest as it emerges upward.
    float intoGroundFade = smoothstep(-0.005, 0, distortion.y);
    
    // Calculate world-relative pulsation values. This will be used to make it look like the magic pulsation effect is gradually moving upwards and fading out.
    float pulseTime = time * 0.5 + worldCoords.x * 4;
    float pulse = frac(pulseTime);
    float pulseCounter = floor(pulseTime);
    
    // Stack sinusoids together based on time and pulsation counter and use the results to determine the opacity of the wave.
    // This ensures that there are certain areas on the ground that have the magic effect, and certain areas that do not, rather than covering absolutely everything.
    // The inclusion of the pulse counter term ensures that the location of those areas that do have the magic effect changes after a pulsation completes.
    float waveCoords = worldCoords.x + pulseCounter * 0.35;
    float wave = smoothstep(1 - electricityCoverage, 1, sin(waveCoords * 120) + sin(waveCoords * 50));
    float pulastionOpacity = sqrt(1 - pulse) * smoothstep(0, 0.1, pulse) * wave;
    
    // Use the aforementioned pulsation and wave to calculation how high up the magic electricity pulsation effect should rise.
    float electricityHeight = (pulse * wave - 0.333) * maxElectricityHeight / screenSize.y;
    
    // Use the coordinate distortions to perform an "is this at the edge of a tile?" check.
    // This ensures that the shape of the magic effect is directly based on the topography of the terrain near the Genesis.
    bool edgeCheck = AtEdge(zoomedCoords + distortion + float2(0, electricityHeight));
    float edgeGlowIntensity = edgeCheck * pulastionOpacity * intoGroundFade;
    
    float distanceFromGenesis = 99999;
    for (int i = 0; i < 15; i++)
    {
        float localDistance = distance(genesisPositions[i], worldPosition) / genesisIntensities[i];
        distanceFromGenesis = min(distanceFromGenesis, localDistance);
    }
    float distanceFadeOpacity = smoothstep(1, 0.8, distanceFromGenesis / glowRadius);
    
    float4 grassData = tex2D(grassDataTexture, coords + screenOffset);
    float depth = saturate(grassData.r - grassData.b * 0.85);
    float vineNoise = smoothstep(0.35, 0, tex2D(vineNoiseTexture, Pixelate(worldCoords * float2(10, 5)))) + 
                      smoothstep(0.15, 0, tex2D(vineNoiseTexture, Pixelate(worldCoords * float2(12, 6.4))));
    float vine = any(tileData) * depth * saturate(vineNoise);
    
    float4 vineColor = float4(0.55, 0.1, 0.23, 1);
    vineColor.rgb *= smoothstep(0.3, 0.9, grassData.r) * smoothstep(0, 0.8, grassData.g);
    
    float4 tileColor = tex2D(baseTexture, coords);
    tileColor = lerp(tileColor, vineColor, vine);
    
    return tileColor + magicColor * edgeGlowIntensity * distanceFadeOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}