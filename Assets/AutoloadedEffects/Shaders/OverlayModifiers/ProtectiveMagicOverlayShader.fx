sampler maskTexture : register(s0);
sampler tileTargetTexture : register(s1);
sampler liquidTargetTexture : register(s2);
sampler noiseTexture : register(s3);

float globalTime;
float universalGlow;
float reciprocalNoiseBrightness;
float reciprocalNoiseFloor;
float2 screenPosition;
float2 screenSize;
float2 zoom;
float2 forcefieldCenter;
float2 keepTopLeft;
float2 keepBottomRight;
float4 pulseColor;
float tileImpactLifetimeRatios[10];
float tileImpactMaxRadii[10];
float2 tileImpactPositions[10];

float2 ConvertToScreenCoords(float2 coords)
{
    return coords * screenSize;
}

float2 ConvertFromScreenCoords(float2 coords)
{
    return coords / screenSize;
}

float4 Sample(float2 coords)
{
    float4 tileTargetData = tex2D(tileTargetTexture, coords);
    
    // Tile edges SHOULD be mostly opaque. The only thing that shouldn't be completely opaque that's in my tile target is LIQUID SLOPES (WHY ARE LIQUID SLOPES HERE WHY WHY WHY SLAM SLAM SLAM).
    // Anyway this exists to make liquids not count as anything for the purposes of outlines and stuff.
    bool fuckYOULiquidSlopes = tileTargetData.a >= 0.5;
    return tex2D(maskTexture, coords) * tileTargetData * fuckYOULiquidSlopes;
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
    float2 pixelationFactor = 2 / screenSize;
    return floor(coords / pixelationFactor) * pixelationFactor;
}

float CalculateImpactGlow(float2 worldPosition)
{
    float baseGlow = 0;
    for (int i = 0; i < 10; i++)
    {
        float maxRadius = tileImpactMaxRadii[i];
        float lifetimeRatio = saturate(tileImpactLifetimeRatios[i]);
        float edgeRadius = sqrt(lifetimeRatio) * maxRadius;
        float maxIntensity = clamp(pow(maxRadius / 60, 1.42), 0, 11) * 10;
        
        float2 impactPosition = tileImpactPositions[i];
        float distanceToEdge = distance(distance(impactPosition, worldPosition), edgeRadius) / screenSize.x;
        float intensity = (1 - lifetimeRatio) * (lifetimeRatio > 0);
        
        baseGlow += intensity / (distanceToEdge + 0.001) * maxIntensity;
    }
    
    return baseGlow * 0.001;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Calculate the position of the given pixel in world space.
    float2 worldPosition = ((position.xy - screenSize * 0.5) / zoom + screenSize * 0.5 + screenPosition);
    float2 worldCoords = Pixelate(worldPosition / screenSize);
    
    // Determine whether the given position is at the edge of the tile range or not.
    // This is used in calculations below to make the edges brighten over time, giving a magical outline look.
    bool beyondKeepEdge = worldPosition.x < keepTopLeft.x || worldPosition.x > keepBottomRight.x ||
                          worldPosition.y < keepTopLeft.y || worldPosition.y > keepBottomRight.y;
    bool atEdge = AtEdge(coords) && worldPosition.x && !beyondKeepEdge;
    
    // Calculate the base color.
    float4 color = Sample(coords);
    
    // Calculate the distance and angle from the forcefield to calculate polar coordinates.
    float distanceFromForcefield = distance(worldPosition, forcefieldCenter);    
    float2 polar = float2(atan2(worldPosition.y - forcefieldCenter.y, worldPosition.x - forcefieldCenter.x) / 6.283 + 0.5, distanceFromForcefield / screenSize.x);
    
    // Calculate the base glow value based on pulsating, radial noise.
    float glow = tex2D(noiseTexture, polar * float2(1, 10) + float2(1, 0.2) * globalTime * 0.1 + float2(polar.y * -5, 0));
    
    // Calculate the impact glow. This accounts for extraneous sources of light, such as a pickaxe mining a block.
    float impactGlowNoiseA = tex2D(noiseTexture, worldCoords * 13);
    float impactGlowNoiseB = tex2D(noiseTexture, worldCoords * 5.5);
    float impactGlowNoiseC = tex2D(noiseTexture, worldCoords * 2.3);
    float impactGlow = CalculateImpactGlow(worldPosition) * pow(impactGlowNoiseA * impactGlowNoiseB * impactGlowNoiseC, 0.333);
    float4 impactGlowColor = float4(1, 1, 1, 0) * impactGlow * color.a * 0.45;
    
    // Combine alternating scrolling noise for the effect below.
    float brightnessQuotient = tex2D(noiseTexture, worldCoords * 6.1 + float2(-0.02, -0.02) * globalTime) * 
                               tex2D(noiseTexture, worldCoords * 4 + float2(0.02, 0.03) * globalTime);
    
    // Apply a reciprocal effect on the brightness quotient to add stronger bits of noise.
    glow += reciprocalNoiseBrightness / (brightnessQuotient + reciprocalNoiseFloor);
    
    // Calculate a pulse effect. This is affected by the distance from the center along with the X position, to give a subtle pulsation effect and horizontal scroll effect.
    float pulse = cos(polar.y * 8 - globalTime * 3 + worldCoords.x * 15) * 0.5 + 0.5;
    
    // Combine everything together.
    float edgeBrightnessInterpolant = smoothstep(0.04, 0.07, length(color.rgb));
    float brightness = dot(color.rgb, 0.333) + atEdge * edgeBrightnessInterpolant * 0.5;
    float4 calculatedColor = saturate(color + pulseColor * glow * brightness * pulse + impactGlowColor * edgeBrightnessInterpolant);
    
    return calculatedColor * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}