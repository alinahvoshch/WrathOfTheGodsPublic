sampler screenTexture : register(s0);

float time;
float distortionIntensity;
float deathRadius;
float whiteGlow;
float2 center;
float2 screenSize;
float2 zoom;
float2 screenPosition;

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 worldPosition = ((position.xy - screenSize * 0.5) / zoom + screenSize * 0.5 + screenPosition);
    
    float distanceFromCenter = distance(worldPosition, center);
    float distortionInterpolant = smoothstep(0.75, 1.33, distanceFromCenter / deathRadius);
    float deathRadiusInterpolant = smoothstep(0.7, 1, distanceFromCenter / deathRadius);
    
    float2 rotatedCoords = RotatedBy(coords - 0.5, time * 2.3 + distanceFromCenter * 0.011) + 0.8;
    coords = lerp(coords, rotatedCoords, distortionInterpolant * distortionIntensity);
    
    return tex2D(screenTexture, coords) + deathRadiusInterpolant * whiteGlow;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
