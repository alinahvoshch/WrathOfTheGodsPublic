sampler baseTexture : register(s0);
sampler riftTexture : register(s1);

float globalTime;
float darkeningRadius;
float pitchBlackRadius;
float2 center;
float2 textureSize;
float3 brightColorReplacement;
float3 bottomLightColorInfluence;

float2 PixelateCoords(float2 coords)
{
    float2 pixelationFactor = 1.5 / textureSize;
    return floor(coords / pixelationFactor) * pixelationFactor;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Calculate polar coordinates.
    float2 pixelatedCoords = PixelateCoords(coords);
    float scaledDistanceFromCenter = length((pixelatedCoords - center) * textureSize / textureSize.y);
    float2 polar = float2(atan2(pixelatedCoords.y - center.y, pixelatedCoords.x - center.x) / 6.283 + 0.5, pow(scaledDistanceFromCenter, 1.7));
    
    // Calculate animated coordinates in accordance with the aforementioned polar coordinates to determine where to sample the rift texture.
    float2 riftCoords = PixelateCoords(polar * float2(3, 16) + globalTime * float2(-0.3, 2.3));
    
    // Sample the rift texture, and apply some darkening in the center, to provide a bit of depth.
    float4 color = tex2D(riftTexture, riftCoords);
    color.rgb *= smoothstep(0, darkeningRadius, scaledDistanceFromCenter - pitchBlackRadius);
    
    // Convert bright white colors to red, in accordance with the Avatar's palette.
    float brightness = length(color);
    float redInterpolant = smoothstep(1.02, 1.32, brightness);
    float4 brightenedSampleColor = lerp(sampleColor, 1, 0.4);
    color = lerp(color * sampleColor, float4(brightColorReplacement, 1) * brightenedSampleColor, redInterpolant);
    
    // The Ceaseless Void has a bright, cyan thing at the bottom of its center.
    // It can be reasonably assumed that this infers light.
    // As such, the bottom of the rift should be recolored.
    float cyanInterpolant = smoothstep(0.5, 0.58, pixelatedCoords.y) * pow(dot(pixelatedCoords - 0.5, float2(0, 1)), 2) * 40;
    color += float4(bottomLightColorInfluence, 0) * cyanInterpolant;
    
    return color * tex2D(baseTexture, coords).a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}