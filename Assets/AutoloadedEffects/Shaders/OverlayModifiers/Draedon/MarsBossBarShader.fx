sampler baseTexture : register(s0);
sampler latticeTexture : register(s1);

float globalTime;
float horizontalSquish;
float3 energyColorA;
float3 energyColorB;
float3 metalColor;
float2 imageSize;
float4 sourceRectangle;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 framedCoords = (coords * imageSize - sourceRectangle.xy) / sourceRectangle.zw;
    float notchInterpolant = smoothstep(0.993, 0.995, framedCoords.x);
    framedCoords.x *= horizontalSquish;
    
    float baseOpacity = tex2D(baseTexture, coords).a;
    float lattice = tex2D(latticeTexture, position.xy * float2(0.3, 1) * 0.0046);
    float latticeInterpolant = cos(lattice * 3.141 + globalTime * 3 - position.x * 0.005) * 0.5 + 0.5;
    
    float3 energyColor = lerp(energyColorA, energyColorB, cos(sqrt(position.x * position.y) * 0.01 - globalTime * 6) * 0.5 + 0.5);
    float3 color = lerp(energyColor, metalColor, latticeInterpolant * 1.2) + notchInterpolant;
    
    return float4(color, 1) * baseOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}