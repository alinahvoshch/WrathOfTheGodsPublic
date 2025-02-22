sampler baseTexture : register(s0);
sampler behindSplitTexture : register(s1);

bool offsetsAreAllowed;
float opacity;
float globalTime;
float splitBrightnessFactor;
float splitTextureZoomFactor;
float2 screenSize;

bool activeSplits[10];
float splitSlopes[10];
float splitWidths[10];
float2 splitCenters[10];
float2 splitDirections[10];

float SignedDistanceToLine(float2 p, float2 linePoint, float2 lineDirection)
{
    return dot(lineDirection, p - linePoint);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate the distance from the screen coordinate to the lines, along with the necessary contributing brightness from that distance.
    float2 offset = 0;
    float brightnessBoost = 0;
    
    for (float i = 0; i < 10; i++)
    {
        if (activeSplits[i])
        {
            float signedLineDistance = SignedDistanceToLine(coords, splitCenters[i], splitDirections[i] * float2(1, screenSize.y / screenSize.x));
            float lineDistance = abs(signedLineDistance);
            float orthogonalDistance = SignedDistanceToLine(coords, splitCenters[i], splitDirections[i].yx * float2(1, screenSize.y / screenSize.x));
            float width = splitWidths[i];
            width *= width >= 8 / screenSize.x;
            
            brightnessBoost += width / (lineDistance + 0.001) * 0.3;
    
            // Calculate how much both sides of the line should be shoved away from the line.
            offset += splitDirections[i] * sign(signedLineDistance) * width * -0.5;
        }
    }
    
    // Calculate colors.
    float4 baseColor = tex2D(baseTexture, coords + offset * offsetsAreAllowed) + brightnessBoost;
    float2 warp = tex2D(behindSplitTexture, coords + globalTime * 0.05).rg * 0.08;
    
    float4 backgroundDimensionColor1 = tex2D(behindSplitTexture, coords * splitTextureZoomFactor + float2(globalTime, 0) * -0.23 - warp) * splitBrightnessFactor;
    float4 backgroundDimensionColor2 = tex2D(behindSplitTexture, coords + backgroundDimensionColor1.rb * splitTextureZoomFactor * 0.32 + warp) * splitBrightnessFactor * 0.5;
    float4 backgroundDimensionColor = backgroundDimensionColor1 + backgroundDimensionColor2;
    backgroundDimensionColor = lerp(backgroundDimensionColor, 1, 0.7);
    
    // Combine colors based on how close the pixel is to the line.
    float brightness = saturate(pow(smoothstep(0.06, 0.4, brightnessBoost), 2));
    return lerp(baseColor, backgroundDimensionColor, brightness);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}