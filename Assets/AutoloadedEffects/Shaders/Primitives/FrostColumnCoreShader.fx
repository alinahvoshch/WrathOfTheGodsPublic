sampler baseTexture : register(s1);
sampler mistNoiseTexture : register(s2);

float generalOpacity;
float localTime;
float endWidthFactor;
float mistInterpolant;
float2 textureSize;
float3 mistColor;
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

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    
    float protrusionFactor = smoothstep(1.3, endWidthFactor, 1 - QuadraticBump(input.TextureCoordinates.y));
    float undulation = (cos(input.TextureCoordinates.y * 18.8495 - localTime * 6) * 0.5 + 0.5) * 0.3;
    float segmentWidth = protrusionFactor + undulation;
    input.Position.x *= segmentWidth;
    
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    float4 color = input.Color;
    
    float cosAngle = coords.x;
    coords.x = acos(cosAngle) / 3.141;
    
    // Use FBM to calculate complex warping offsets in the noise texture that will be used to create the fog noise.
    float2 noiseOffset = 0;
    float noiseAmplitude = 0.03;
    float2 noiseZoom = 3.187;
    float2 worldAdjustedCoords = coords * float2(0.25, 0.6);
    worldAdjustedCoords.y += coords.x * 0.1;
    
    for (float i = 0; i < 2; i++)
    {
        float2 scrollOffset = float2(localTime * 0.185 - i * 0.338, localTime * -0.533 + i * 0.787) * noiseZoom.y * 0.04;
        noiseOffset += (tex2D(mistNoiseTexture, worldAdjustedCoords * noiseZoom + scrollOffset) - 0.5) * noiseAmplitude;
        noiseZoom *= 2;
        noiseAmplitude *= 0.5;
    }
    
    // Calculate warp noise values.
    float sourceDistanceWarp = tex2D(mistNoiseTexture, worldAdjustedCoords * 3 - float2(0, localTime * 0.67));
    
    // Calculate noise values.
    noiseOffset.x += localTime * 0.21 + coords.y * 0.4;
    float noise = lerp(tex2D(mistNoiseTexture, worldAdjustedCoords * 0.61 + noiseOffset).r, 0.5, 0.4);
    float circleEdgeNoise = tex2D(mistNoiseTexture, worldAdjustedCoords * float2(1.7, 5) + noiseOffset).r;
    float darknessEdgeNoise = tex2D(mistNoiseTexture, worldAdjustedCoords * 4.1 + noiseOffset * 3).r;
    
    // Calculate the base color and its brightness.
    float4 baseColor = tex2D(baseTexture, coords);
    float brightness = dot(baseColor.rgb, float3(0.3, 0.59, 0.11));
    
    // Combine the base color with the fog.
    float4 result = lerp(baseColor, noise * float4(mistColor, 1), pow(mistInterpolant, 1.25) * 0.96);
    result += result.a * length(mistColor) * 0.3 / darknessEdgeNoise;
    
    float edgeOpacity = smoothstep(1, 0.9, abs(input.TextureCoordinates.x) + circleEdgeNoise * 0.08);
    float opacity = edgeOpacity * smoothstep(0, 0.1, coords.y - darknessEdgeNoise * 0.05) * smoothstep(1, 0.8, coords.y + darknessEdgeNoise * 0.1);
    
    return (result * opacity * color) * generalOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
