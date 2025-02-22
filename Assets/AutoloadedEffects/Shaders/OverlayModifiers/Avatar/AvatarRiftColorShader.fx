sampler baseTexture : register(s0);
sampler overlayTexture : register(s1);
sampler ditherPattern : register(s2);
sampler ditherMask : register(s3);

float time;
float suckSpeed;
float blurOffset;
float scaleCorrection;
float blurWeights[5];
float2 worldPositionOffset;

float4 DitherColor(float2 coords, float2 baseCoords, float ditherChance, float2 scroll)
{
    // Calculate the dither value by sampling a noisy checkerboard pattern texture.
    float pattern = tex2D(ditherPattern, baseCoords * 120 + float2(0, time * -0.5));
    float ditherValue = pattern * ditherChance - any(blurOffset);
    float4 color = tex2D(overlayTexture, coords / scaleCorrection + scroll);
    
    // If the dither value exceeds a specific threshold, eliminate the pixel to give the impression of dithering.
    if (ditherValue >= 0.56)
        return 1 - pattern;
    
    // Return the base color.
    return color;
}

float SmoothLoop(float loopSpeed)
{
    return sin(-time + loopSpeed * 6.283) * loopSpeed;
}

float2 ToPolar(float2 cartesian)
{
    // Converts cartesian coordinates to polar, storing the angle in the X axis and the distance in the Y axis.
    float angle = atan2(cartesian.y, cartesian.x);
    float distance = length(cartesian);
    return float2(angle, distance);
}

float2 ToCartesian(float2 polar)
{
    // Converts polar coordinates back to cartesian.
    float angle = polar.x;
    float distance = polar.y;
    return float2(cos(angle), sin(angle)) * distance;
}

float4 GetBlurredRiftData(float2 coords)
{
    float4 blurColor = 0;
    for (int i = -2; i < 3; i++)
    {
        for (int j = -2; j < 3; j++)
        {
            float weight = blurWeights[abs(i) + abs(j)];
            blurColor += tex2D(baseTexture, coords + float2(i, j) * blurOffset) * weight;
        }
    }
    
    return blurColor;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 baseCoords = coords;
    
    // Black = Fill.
    // Red = Edge.
    // Green = Dither intensity.
    // Blue = Downward scroll speed.
    // Alpha = Alpha.
    float4 riftData = GetBlurredRiftData(coords);
    float opacity = riftData.a;
    float scrollData = riftData.b;
    float4 edgeAdditive = float4(0.7, 0, -0.28, 0) * smoothstep(0.7, 1, riftData.r) * 0.5;
    
    // Calculate coordinate-based values.
    float scrollSpeed = scrollData * coords.y;
    float2 scroll = float2(0, SmoothLoop(scrollSpeed));
    float2 baseOverlayCoords = (coords - 0.5) * 2;
    float inwardZoom = lerp(1, 3, smoothstep(0, 0.32, distance(coords, 0.5)));
    float spinTime = time * (suckSpeed + 1);
    float2 polarOverlayCoords = ToPolar(baseOverlayCoords * inwardZoom);
    polarOverlayCoords.x += spinTime * 1.34 - polarOverlayCoords.y * 0.6 + length(worldPositionOffset) * 0.2 + sin(polarOverlayCoords.y * 6 - spinTime) * 0.7;
    polarOverlayCoords.y = fmod(polarOverlayCoords.y, 1.414);
    baseOverlayCoords = ToCartesian(polarOverlayCoords) * 0.6;
    
    // Calculate the overlay color. This takes dithering into account.
    // Dithering is stronger for translucent pixels.
    float ditherChance = riftData.g * tex2D(ditherMask, coords * 0.6 + float2(0, time * 0.1)).r + (1 - riftData.a);
    float4 overlayColor = DitherColor(baseOverlayCoords, baseCoords, ditherChance, scroll + worldPositionOffset + float2(time * 0.35, 0));
    
    // Darken colors via an exponent.
    overlayColor = pow(overlayColor, 1.5);
    
    // Calculate the darkness interpolant based on radial distance from center. Colors closer to the center are darker.
    float distanceFromCenter = distance(coords, 0.5) / scaleCorrection;
    float darknessInterpolant = smoothstep(0.23, 0.06, distanceFromCenter);
        
    // Apply red colors the further out the pixel is from the center. This effect is diminished for parts of the texture that are scrolling down, such as liquids.
    // To ensure variety, this effect requires the original pixel color to have some amount of green to it.
    float redInterpolant = smoothstep(0.14, 0.34, distanceFromCenter) * overlayColor.g - scrollData * 30;
    overlayColor += float4(0.67, -0.3, -0.6, 0) * saturate(redInterpolant);
    
    // Apply cyan colors based on scroll values. This chiefly ensures that colors on the liquid part of the texture are slightly different.
    float cyanInterpolant = smoothstep(0, 0.54, riftData.g) * overlayColor.g * 4.3;
    overlayColor = lerp(overlayColor, float4(0.7, 0.64, 1, 1), cyanInterpolant);
    
    // Calculate the aforementioned color from the darkness interpolant and combine everything.
    float4 darknessMask = float4(1 - darknessInterpolant, 1 - darknessInterpolant, 1 - darknessInterpolant, 1);
    
    float edgeBrightness = 1 + riftData.r * smoothstep(1, 0.75, riftData.r) * 2;
    return (overlayColor + edgeAdditive) * sampleColor * darknessMask * opacity * edgeBrightness * 1.6;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}