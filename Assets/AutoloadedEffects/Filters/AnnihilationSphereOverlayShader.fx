sampler baseTexture : register(s0);
sampler sphereTargetTexture : register(s1);

float globalTime;
float2 spherePosition;
float2x2 projection;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float4 sphereColor = tex2D(sphereTargetTexture, mul(coords - 0.5, projection) + 0.5);
    float blue = sphereColor.g * sphereColor.b - sphereColor.r;
    
    float2 directionFromSphere = normalize(position.xy - 0.5);
    float tangentAmplitudeFactor = lerp(0.5, 1.5, sin(globalTime * 8 + blue * 3) * 0.5 + 0.5);
    float2 tangentFromSphere = float2(ddx(blue), ddy(blue)) * 0.2 + float2(directionFromSphere.y, directionFromSphere.x) * sin(blue * 30) * 0.05;
    
    coords += tangentFromSphere * tangentAmplitudeFactor;
    
    return tex2D(baseTexture, coords);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}