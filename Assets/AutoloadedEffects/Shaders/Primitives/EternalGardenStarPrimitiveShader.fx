sampler starTexture : register(s1);

float opacity;
float glowIntensity;
float globalTime;
float distanceFadeoff;
float minTwinkleBrightness;
float maxTwinkleBrightness;
float recedeDistance;
float2 screenSize;
float2 eyePosition;
matrix projection;

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
    
    float2 recedeOffset = (input.Position.xy - eyePosition) * recedeDistance;
    input.Position += float4(recedeOffset, 0, 0);
    
    float4 pos = mul(input.Position, projection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Calculate various coordinates in advance.
    float2 coords = input.TextureCoordinates;
    float2 position = input.Position.xy;
    float2 screenCoords = position / screenSize;
    float twinkle = lerp(minTwinkleBrightness, maxTwinkleBrightness, cos(length(coords - 0.5) * 5.25 + globalTime * 1.4) * 0.5 + 0.5);
    
    // Calculate the base color of the star.
    float4 color = input.Color * tex2D(starTexture, coords);
    
    float distanceFromCenter = distance(coords, 0.5) + distance(twinkle, 1) * 0.05;
    float colorExponent = lerp(7.5, 2, opacity);
    float4 baseColor = pow(color, colorExponent) * 3 + smoothstep(0.12, 0.01, distanceFromCenter);
    baseColor *= pow(smoothstep(0.25, 0, distanceFromCenter), 2);
    
    float brightness = dot(color.rgb, 0.333);
    
    return saturate(baseColor) * twinkle * smoothstep(-0.3, 0, brightness - (1 - opacity));
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
