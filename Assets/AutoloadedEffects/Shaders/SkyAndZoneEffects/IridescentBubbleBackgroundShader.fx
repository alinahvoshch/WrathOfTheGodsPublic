sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);
sampler iridescenceTexture : register(s2);

float globalTime;
float pastelInterpolant;
float posterizationDetail;
float bubbleRadius;
float iridescenceScrollSpeed;
float lightStripIntensity;
float darknessStripIntensity;
float iridescenceZoomFactor;
float wobbleIntensity;
float2 pixelation;
float2 resolution;
float2 highlightPosition;
float3 bubbleCenter;

/*
        // Original parameters:
        ManagedShader bubbleShader = ShaderManager.GetShader("NoxusBoss.IridescentBubbleBackgroundShader");
        bubbleShader.TrySetParameter("pastelInterpolant", 0.19f);
        bubbleShader.TrySetParameter("posterizationDetail", 30f);
        bubbleShader.TrySetParameter("bubbleRadius", 0.41f);
        bubbleShader.TrySetParameter("iridescenceScrollSpeed", 0.051f);
        bubbleShader.TrySetParameter("pixelation", Vector3.One * 2f);
        bubbleShader.TrySetParameter("highlightPosition", new Vector2(0.15f, -0.14f));
        bubbleShader.TrySetParameter("bubbleCenter", new Vector3(0.5f, 0.5f, 0f));
        bubbleShader.TrySetParameter("lightStripIntensity", 3f);
        bubbleShader.TrySetParameter("iridescenceZoomFactor", 1f);
        bubbleShader.TrySetParameter("darknessStripIntensity", 0.29f);
        bubbleShader.TrySetParameter("wobbleIntensity", 0.004f);
        bubbleShader.TrySetParameter("resolution", ViewportSize);
        bubbleShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        bubbleShader.SetTexture(GennedAssets.Textures.Extra.Psychedelic, 2, SamplerState.LinearWrap);
        bubbleShader.Apply();
*/

float FixedDistance(float2 a, float2 b)
{
    float2 resolutionAdjustedA = (a - 0.5) * float2(resolution.x / resolution.y, 1) + 0.5;
    return distance(resolutionAdjustedA, b);
}
float FixedDistance(float3 a, float3 b)
{    
    float3 resolutionAdjustedA = (a - 0.5) * float3(resolution.x / resolution.y, 1, 1) + 0.5;
    return distance(resolutionAdjustedA, b);
}

float2 GetRaySphereIntersectionOffsets(float3 rayOrigin, float3 rayDirection, float3 spherePosition, float sphereRadius)
{
    float3 offsetFromSphere = rayOrigin - spherePosition;
    float a = dot(rayDirection, rayDirection);
    float b = dot(rayDirection, offsetFromSphere) * 2;
    float c = dot(offsetFromSphere, offsetFromSphere) - (sphereRadius * sphereRadius);
    float discriminant = b * b - a * c * 4;
    
    return float2(-b - sqrt(discriminant), -b + sqrt(discriminant)) / (a * 2);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Wobble about slightly.
    float2 originalCoords = coords;
    coords.x += sin(coords.y * 19 + globalTime * 7.2) * wobbleIntensity;

    // Apply pixelation.
    float2 pixelationFactor = pixelation / resolution;
    coords = round(coords / pixelationFactor) * pixelationFactor;
    
    float3 rayDirection = float3(0, 0, 1);
    float3 rayOrigin = float3(coords, -2);
    float2 intersectionDistances = GetRaySphereIntersectionOffsets(rayOrigin, rayDirection, bubbleCenter, bubbleRadius);    
    float2 polar = float2(atan2(coords.y, coords.x) / 6.283 + 0.5, distance(coords, 0.5));
    
    float3 light = 0;
    for (float i = 0; i < 18; i++)
    {
        // Sample along the bubble, taking even steps along the bubble's interior, starting at the front and ending at the back.
        float3 samplePosition = rayOrigin + rayDirection * lerp(intersectionDistances.x, intersectionDistances.y, i / 17);
        
        // Determine how far away the sample point is from the edge of the bubble.
        // The edge will be given fake iridescence, while the interior will be given nothing, similar to how one would expect a bubble to work.
        float distanceFromCenter = FixedDistance(samplePosition, bubbleCenter);
        float distanceFromEdge = distance(distanceFromCenter, bubbleRadius * 0.92);
        
        // Calculate a warp offset value based on noise, and then determine the iridescent color.
        float2 warp = tex2D(noiseTexture, coords * 0.4 + float2(0, 0.05) * globalTime).r * float2(0.05, 0);
        float2 iridescenceZoom = float2(2, samplePosition.z + 0.4) * iridescenceZoomFactor;
        float2 iridescenceScroll = float2(sign(samplePosition.z) * iridescenceScrollSpeed, 0) * globalTime + warp.yx;
        float3 iridescence = tex2D(iridescenceTexture, polar * iridescenceZoom + iridescenceScroll);
        
        // Interpolate towards the greyscale representation of the iridescent color based on a pastel interpolant, to make the rainbows a bit less lurid.
        iridescence = lerp(iridescence, dot(iridescence, float3(0.3, 0.6, 0.1) + 0.1), pastelInterpolant);
        
        // Ensure that only the edges receive light.
        float edgeGlow = saturate(0.03 / distanceFromEdge) * smoothstep(0.03, 0, distanceFromEdge);
        
        // Make the backside of the bubble provide less light.
        // Enough light that the 3D motion can be seen from the back, but not so much that it seems unnatural.
        float backsideFade = smoothstep(1, 0, samplePosition.z);
        
        // Calculate the influence of bright and dark vertical strips that flow along the bubble, similar to the reference animation.
        // The bright strip's position is influenced by noise.
        float3 stripGlow = pow(sin(samplePosition.y * 13 + pow(samplePosition.x - 0.5, 2) * 25 + globalTime * 8 + warp.x * 50) * 0.5 + 0.5, 40) * iridescence * lightStripIntensity;
        float darkness = pow(sin(samplePosition.y * 6 + pow(samplePosition.x - 0.5, 2) * 20 + globalTime * 2 - warp.x * 6) * 0.5 + 0.5, 20) * darknessStripIntensity;
        
        light += (iridescence + stripGlow - darkness) * backsideFade * edgeGlow;
    }
    
    // Calculate the brightness for a color at the very edge of the bubble in 2D space, to give an outline to the overall shape.
    float edgeDistance = distance(FixedDistance(float3(coords, 0), bubbleCenter), bubbleRadius * 0.95);
    float brightEdge = smoothstep(0.02, 0, edgeDistance);

    // Apply posterization.
    light = round(light * posterizationDetail) / posterizationDetail;
    
    // Apply a highlight to the bubble.
    float2 absoluteHighlightPosition = bubbleCenter.xy + highlightPosition;
    float highlight = pow(smoothstep(0.24, 0.01, FixedDistance(originalCoords, absoluteHighlightPosition)), 2);
    
    return sampleColor + float4(light, 0) + brightEdge + highlight;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}