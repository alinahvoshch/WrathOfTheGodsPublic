sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float globalTime;
float gradientCount;
float huePower;
float horizontalSquish;
float2 imageSize;
float4 sourceRectangle;
float3 gradient[5];

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = clamp(startIndex + 1, 0, gradientCount - 1);
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 framedCoords = (coords * imageSize - sourceRectangle.xy) / sourceRectangle.zw;
    float notchInterpolant = smoothstep(1 - 0.005 / horizontalSquish, 1 - 0.003 / horizontalSquish, framedCoords.x);
    framedCoords.x *= horizontalSquish;
    
    float4 basePixel = tex2D(baseTexture, coords);
    float luminosity = (basePixel * sampleColor).r;
    float timeHueOffset = cos(globalTime * 0.8 + framedCoords.x * 6.283) * 0.05;
    float hue = cos(luminosity * 3.141 - (1 - framedCoords.x) * 3.141) * 0.5 + 0.5 + timeHueOffset;
    
    float3 color = PaletteLerp(pow(hue, huePower)) + notchInterpolant;
    
    return float4(color, 1) * basePixel.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}