sampler baseTexture : register(s0);
sampler riftTexture : register(s1);
sampler cutoutNoiseTexture : register(s2);

float erasureInterpolant;
float time;
float darkeningRadius;
float pitchBlackRadius;
float redEdgeBuffer;
float2 center;
float2 textureSize;
float3 brightColorReplacement;
float3 bottomLightColorInfluence;

float2 PixelateCoords(float2 coords)
{
    float2 pixelationFactor = 1.2 / textureSize;
    return floor(coords / pixelationFactor) * pixelationFactor;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Calculate polar coordinates.
    float2 pixelatedCoords = PixelateCoords(coords);
    float scaledDistanceFromCenter = length((pixelatedCoords - center) * textureSize / textureSize.y);
    float2 polar = float2(atan2(pixelatedCoords.y - center.y, pixelatedCoords.x - center.x) / 6.283 + 0.5, pow(scaledDistanceFromCenter, 1.7));
    polar.x += scaledDistanceFromCenter * 4;
    
    // Calculate animated coordinates in accordance with the aforementioned polar coordinates to determine where to sample the rift texture.
    float2 riftCoords = PixelateCoords(polar * float2(3, 16) + time * float2(-0.3, 2.3));
    
    // Sample the rift texture, and apply some darkening in the center, to provide a bit of depth.
    float4 color = tex2D(riftTexture, riftCoords);
    color.rgb *= smoothstep(0, darkeningRadius, scaledDistanceFromCenter - pitchBlackRadius);
    
    // Convert bright white colors to red, in accordance with the Avatar's palette.
    float brightness = length(color);
    float redInterpolant = smoothstep(1.02, 1.32, brightness);
    
    // Erase pixels in accordance with distance from the center, along with noise.
    float edgeErasureNoise = tex2D(cutoutNoiseTexture, polar * float2(1, 0.2) + float2(-1, 0.2) * time + (coords - 0.5) * 0.2);
    float erasureDistanceThreshold = 0.43 + (edgeErasureNoise - 0.75) * 0.3 - erasureInterpolant * 0.7;
    bool erasePixel = scaledDistanceFromCenter >= erasureDistanceThreshold;
    
    // Bias pixels close to erasure towards red as well.
    redInterpolant = saturate(redInterpolant + smoothstep(0.98 - redEdgeBuffer, 0.98, scaledDistanceFromCenter / erasureDistanceThreshold));
    
    // Apply red-biasing.
    color = lerp(color, float4(brightColorReplacement, 1), redInterpolant);
    
    return color * sampleColor * (1 - erasePixel);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}