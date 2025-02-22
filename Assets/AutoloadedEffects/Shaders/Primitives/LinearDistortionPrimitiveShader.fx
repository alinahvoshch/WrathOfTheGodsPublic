sampler flameStreakTexture : register(s1);

float directionInterpolant;
float intensityFactor;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
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

// This is nearly equivalent to the ImpFlameTrail shader I wrote for Calamity a while back.
// It is duplicated so as to be usable by the custom ManagedShader system.
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates;
    
    // Account for texture distortion artifacts in accordance with the primitive distortion fixes.
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    float direction = frac(directionInterpolant);
    float distanceFromCenter = distance(coords.y, 0.5);
    
    // Determine how strong the intensity of distortions should be, tapering at the start and end of the primitive line.
    float intensity = smoothstep(0.1, 0.2, coords.x) * smoothstep(0.95, 0.8, coords.x) * intensityFactor * color.a;

    return float4(direction, (0.5 - distanceFromCenter) * intensity * 2, 1, 1);
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
