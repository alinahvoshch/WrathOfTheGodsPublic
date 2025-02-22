sampler baseTexture : register(s0);
sampler itemMaskTexture : register(s1);

float globalTime;
float distortionStrength;
float distortionAreaFactor;
float2 loreItemPositions[10];
float2 oldScreenPosition;
float2 screenPosition;
float2 screenSize;
float2 zoom;

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float minDistance = 99999;
    for (int i = 0; i < 10; i++)
        minDistance = min(minDistance, distance(position.xy, loreItemPositions[i]));
    
    float2 screenOffset = (screenPosition - oldScreenPosition) / screenSize * zoom;
    
    float distortionAngle = exp(-minDistance / distortionAreaFactor) * distortionStrength * 10;
    float2 distortedCoords = RotatedBy(coords - 0.5, distortionAngle) + 0.5;
    float4 itemDataColor = tex2D(itemMaskTexture, coords + screenOffset);
    
    float conversionInterpolant = smoothstep(0, 1, length(itemDataColor.rgb) + itemDataColor.a);
    
    return lerp(tex2D(baseTexture, distortedCoords), itemDataColor, conversionInterpolant);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}