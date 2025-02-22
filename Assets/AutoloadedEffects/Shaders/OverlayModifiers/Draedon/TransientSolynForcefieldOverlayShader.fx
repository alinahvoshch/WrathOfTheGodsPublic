sampler baseTexture : register(s0);

float globalTime;
float zoom;
float pulse;
float safeZoneWidth;
float2 solynPosition;
float2 laserDirection;

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 offsetFromSolyn = solynPosition - position.xy;
    float2 directionToSolyn = normalize(offsetFromSolyn);
    float normalizedAngle = acos(dot(directionToSolyn, laserDirection)) * length(offsetFromSolyn);
    
    float safeZoneEdge = zoom * safeZoneWidth * 0.5;
    float safeZoneGlow = QuadraticBump(smoothstep(0.8 - pulse, 1.2 + pulse, normalizedAngle / safeZoneEdge)) * 1.5;
    bool inSafeZone = normalizedAngle <= safeZoneEdge;
    
    float4 baseColor = tex2D(baseTexture, coords);
    return baseColor * (1 - inSafeZone) + safeZoneGlow * pow(baseColor.a, 3);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}