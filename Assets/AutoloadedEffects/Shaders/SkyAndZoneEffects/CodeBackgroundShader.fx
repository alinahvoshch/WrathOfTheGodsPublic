sampler binaryTexture : register(s1);

float globalTime;
float digitShiftSpeed;
float opacityScrollSpeed;
float downwardScrollSpeed;
float totalFrames;
float2 frameSize;
float2 viewSize;
float4 binaryColor;

float Random(float2 coords)
{
    return frac(sin(dot(coords, float2(152.9898, 378.233))) * 59000);
}

float4 CalculateBinaryLayerColor(float2 baseCoords, float layerZoom)
{
    baseCoords.y += layerZoom * 0.1;
    
    // Calculate a random scroll speed factor that varies by latter on the X axis.
    float2 snapFactor = frameSize / viewSize / layerZoom;
    float scrollSpeedFactor = 1 + Random((floor(baseCoords * float2(1, totalFrames) / snapFactor) * snapFactor).y);
    
    // Apply the scroll effect.
    baseCoords.x -= globalTime * downwardScrollSpeed * scrollSpeedFactor / pow(layerZoom + 1, 1.5);
    
    // Normalize coordinates.
    float2 coords = baseCoords / snapFactor;
    
    // Initialize the letter as a 0 by ensuring it doesn't pass into the next frame.
    coords.y %= 1 / totalFrames;
    
    // Snap coordinates to the nearest digit space.
    float2 snappedCoords = floor(baseCoords / snapFactor) * snapFactor;
    
    // Determine whether the letter should display a 1 instead of a 0 based on noise and a time scroll.
    float oneDisplayNoise = Random(snappedCoords);    
    float displayFrameSinusoid = cos(oneDisplayNoise * 6.283 + globalTime * digitShiftSpeed) * 0.5 + 0.5;
    float displayFrame = round(displayFrameSinusoid * totalFrames);
    coords.y += displayFrame * totalFrames;
    
    // Calculate opacity based on noise and a time scroll.
    float opacityNoise = Random(snappedCoords + 0.441);
    float opacity = cos(opacityNoise * 20 + globalTime * opacityScrollSpeed) * 0.5 + 0.5;
    
    return (tex2D(binaryTexture, coords) - float4(30, 30, 30, 0) / 255) * opacity;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Combine three layers of scrolling to compose the effect.
    float4 layerA = CalculateBinaryLayerColor(coords, 1.1);
    float4 layerB = CalculateBinaryLayerColor(coords, 1.9);
    float4 layerC = CalculateBinaryLayerColor(coords, 3.1) * 0.5;
    float4 layerColor = (layerA + layerB + layerC) * binaryColor;
    
    return sampleColor + layerColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}