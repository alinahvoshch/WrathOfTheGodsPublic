sampler noiseTexture : register(s1);

float globalTime;
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

float hash12(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 30000);
}

float CalculateStarBrightness(float2 coords, float cutoffThreshold)
{
    float brightness = hash12(coords);
    if (brightness >= cutoffThreshold)
    {
        brightness = pow((brightness - cutoffThreshold) / (1 - cutoffThreshold), 26);
        float twinkle = cos(globalTime * 5 + brightness * 150) * 0.5;
        brightness *= (1 + twinkle);
    }
    else
        brightness = 0.0;
    
    return brightness;
}

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates;
    
    // Account for texture distortion artifacts in accordance with the primitive distortion fixes.
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    float edgeProximity = QuadraticBump(coords.y);
    float innerGlow = smoothstep(0.6, 1, edgeProximity);
    color += float4(-1, 1, 1, -0.5) * innerGlow * color.a * 0.25;
    color *= lerp(1, 0.32, innerGlow);
    color += innerGlow * CalculateStarBrightness(input.Position.xy / 1250, 0.95);
    
    // Fade the telegraph at the edges.
    color *= pow(smoothstep(0, 0.6, edgeProximity), 1.5);
    
    // Add a strong starting glow.
    color = saturate(color + smoothstep(0.09, 0.03, coords.x) * color.a * 1.5);
    
    // Fade the very start.
    color *= smoothstep(0.02, 0.056, coords.x);
    
    // Fade at the end.
    color *= smoothstep(0.3, 0.12, coords.x);
    
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
