sampler shapeNoiseTexture : register(s1);
sampler colorHighlightTexture : register(s2);

float localTime;
float horizontalSpinDirection;
float zoom;
float noiseCutoffThreshold;
float highlightGlowFactor;
float2 windSpeedFactor;
float4 baseColor;
float4 backColor;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    float2 baseCoords = input.TextureCoordinates;
    
    float cosAngle = coords.x;
    coords.x = acos(cosAngle) / 3.141;
    
    // Apply scrolling that gives the impression of fast-moving winds along the tornado mesh.
    coords.x += (coords.y * horizontalSpinDirection * -4.4 - horizontalSpinDirection * localTime * windSpeedFactor.x * 2.3) * 0.87;
    coords.y += coords.x * horizontalSpinDirection * -0.6 - localTime * windSpeedFactor.y * 0.98;
    
    // Apply noise-angle warping, to give a bit more life to the resulting colorations.
    float noiseAngle = tex2D(colorHighlightTexture, coords) * 16 + coords.y * 20;
    coords += float2(cos(noiseAngle), sin(noiseAngle)) * 0.03;
    
    // Apply zoom effects.
    coords *= zoom;
    
    // Calculate the noise value.
    float noise = sqrt(tex2D(shapeNoiseTexture, coords) * tex2D(shapeNoiseTexture, coords * 0.8));
    
    // Make noise vanish near the vertical end-points of the tornado.
    noise -= smoothstep(0.2, 0, baseCoords.y) * 0.5 + smoothstep(0.9, 1, baseCoords.y) * 0.45;
    
    // Ensure that low noise values, along with the horizontal edges, fade out.
    // This helps significantly with establishing depth.
    float opacity = smoothstep(1, 0.67, abs(cosAngle)) * smoothstep(0.1, 0.12, noise);
    
    // Calculate the glow intensity. This is stronger near the center, to help suggest depth.
    float glow = 1 + smoothstep(0.75, 0.1, abs(cosAngle)) * highlightGlowFactor;
    
    return lerp(baseColor, backColor * 0.15, smoothstep(noiseCutoffThreshold + 0.2, noiseCutoffThreshold, noise)) * opacity * glow;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
