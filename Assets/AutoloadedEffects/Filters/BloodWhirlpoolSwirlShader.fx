sampler screenTexture : register(s0);

bool performSwirl;
float globalTime;
float overlayInterpolant;
float animationCompletion;
float3 overlayColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float centerDistance = distance(coords, 0.5);
    float angle = atan2(coords.y - 0.5, coords.x - 0.5);
    
    float2 offset = float2(0.5, 0.67);
    float2 centeredCoords = coords - offset;
    
    float swirlRotation = length(centeredCoords) * pow(animationCompletion, 3) * performSwirl * 42;
    float swirlSine = sin(swirlRotation);
    float swirlCosine = cos(swirlRotation);
    float2x2 swirlRotationMatrix = float2x2(swirlCosine, -swirlSine, swirlSine, swirlCosine);
    float2 swirlCoordinates = mul(centeredCoords, swirlRotationMatrix) + offset;
    
    float4 baseColor = tex2D(screenTexture, swirlCoordinates);
    baseColor.rgb = lerp(baseColor, overlayColor, overlayInterpolant);
    
    return baseColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
