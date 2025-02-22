sampler2D baseTexture : register(s0);

float globalTime;
float waveMoveSpeed;
float wavePeriod;
float centerPartInterpolant;
float forcefieldRadius;
float2 forcefieldPosition;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float wave = sin(coords.x * wavePeriod + globalTime * waveMoveSpeed) * 0.5 + 0.5;
    wave *= sin(coords.x * wavePeriod * 1.3 + globalTime * waveMoveSpeed * -1.8) * 0.5 + 0.5;
    wave += (sin(coords.x * wavePeriod * 5.1 + globalTime * waveMoveSpeed * 3.8) * 0.5 + 0.5) * 0.09;
    
    bool erasePixel = coords.y <= wave * 0.003;
    float opacity = smoothstep(0.97, 1, distance(position.xy, forcefieldPosition) / forcefieldRadius);
    
    float horizontalDistanceFromCenter = distance(coords.x, 0.5);
    float partWave = sin(coords.y * wavePeriod * 10.8 + globalTime * waveMoveSpeed + coords.x * 0.5) * 0.5 + 0.5;
    opacity *= smoothstep(0, 0.01, horizontalDistanceFromCenter - centerPartInterpolant + partWave * 0.08 + 0.01);
    
    return tex2D(baseTexture, coords) * sampleColor * (1 - erasePixel) * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}