sampler screenTexture : register(s0);

float globalTime;
float intensity;
float distortionIntensity;
float2 screenSize;
float2 vignetteSource;

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float vignetteDistance = distance(position.xy, vignetteSource);
    float vignetteFade = smoothstep(100, 200, vignetteDistance);
    float brightness = 1 - sqrt(intensity * vignetteFade) * 0.01;
    
    float2 vignetteSourceUV = vignetteSource / screenSize;
    float2 rotatedCoords = RotatedBy(coords - vignetteSourceUV, vignetteDistance * -0.045 + globalTime * 5) + vignetteSourceUV;
    
    coords = lerp(coords, rotatedCoords, smoothstep(distortionIntensity * 360 + 0.01, 0, vignetteDistance) * smoothstep(50, 100, vignetteDistance));
    
    return tex2D(screenTexture, coords) * float4(brightness, brightness, brightness, 1);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
