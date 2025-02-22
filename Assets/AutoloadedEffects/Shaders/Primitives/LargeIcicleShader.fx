sampler noiseTexture : register(s1);

float localTime;
float reach;
float identity;
float3 generalColor;
float3 viewPosition;
float3 lightDirection;
matrix uWorldViewProjection;
matrix rotation;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
    float3 Normal : NORMAL0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    
    input.Position.x += sin(input.Position.y * 16 + identity) * 0.06;
    input.Position.z += sin(input.Position.y * 19 + identity * 1.5) * 0.03;
    
    float4 pos = mul(input.Position * float4(1 - input.TextureCoordinates.y, 1, 1 - input.TextureCoordinates.y, 1), uWorldViewProjection);
    
    output.Position = pos;
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    output.Normal = normalize(input.Normal);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = floor(input.TextureCoordinates * 150) / 150;
    float3 normal = normalize(input.Normal);
    
    float lightingInterference = tex2D(noiseTexture, coords) - 0.5;
    float backlightNoise = tex2D(noiseTexture, coords * 0.67) - 0.5;
    
    float4 color = input.Color * float4(generalColor, 1);
    float brightness = pow(saturate(dot(normal, lightDirection)) + lightingInterference * 0.2, 1.2) + 0.07 + backlightNoise * 0.12;
    color.rgb *= brightness;
    
    float3 viewDirection = normalize(float3(float2(viewPosition.xy - input.Position.xy), 0));
    float3 reflectDirection = reflect(normal, -lightDirection);
    float specular = pow(saturate(dot(viewDirection, reflectDirection) + lightingInterference * brightness * 0.92), 1.4 - lightingInterference * 2);
    color.rgb *= 1 + specular * 1.67;
    color.b += color.a * smoothstep(0.15, 0.03, brightness) * 0.1;
    
    color.rgb *= lerp(float3(0.18, 0, 0.097), 1, smoothstep(0, 0.045, coords.y - reach));
    
    return saturate(color) * smoothstep(0, 0.011, coords.y);
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
