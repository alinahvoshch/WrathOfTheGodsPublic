sampler baseTexture : register(s0);
sampler screenTexture : register(s1);

float globalTime;
float aspectRatio;
float2 avatarUV;

float4 InfiniteMirrorEffect(float maxIterations, float2 coords)
{
    float breakOutIteration = maxIterations;
    for (int i = 0; i < maxIterations; i++)
    {
        if (coords.x < 0.05 || coords.x > 0.95 || coords.y < 0.05 || coords.y > 0.95)
        {
            breakOutIteration = i;
            break;
        }

        coords = (coords - 0.5) * 1.34 + 0.5;
        coords.x += sin(i * 0.4 + globalTime) * 0.003;
    }
    
    return tex2D(screenTexture, coords) * (1 - breakOutIteration / maxIterations);
}

float CalculateMandelbrotInterpolant(float2 coords, float maxIterations)
{
    float2 z = 0;
    for (int i = 0; i < maxIterations; i++)
    {
        z = float2(dot(z, z * float2(1, -1)), z.x * z.y * 2) + coords;
        if (length(z) > 2)
            return i / maxIterations;
    }
    
    return 1;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float innerColorInterpolant = 1;
    
    float4 reflectionColor = InfiniteMirrorEffect(25, coords);
    
    float zoom = pow(0.025, 0.85 + sin(globalTime * 0.19)) * 0.4;
    
    float2 startingPoint = float2(-0.483, 0.6255);
    
    float2 mandelbrotCoords = startingPoint + (coords - avatarUV) * float2(aspectRatio, 1) * zoom;
    float mandelbrotInterpolant = CalculateMandelbrotInterpolant(mandelbrotCoords, 96);
    mandelbrotInterpolant += tex2D(baseTexture, frac(coords * 4 + reflectionColor.xy * 0.1)).r * lerp(0.2, 1.2, mandelbrotCoords) * smoothstep(1, 0.97, mandelbrotInterpolant) * 0.41;
    
    float4 mandelbrotColor = float4(cos(3.0 + mandelbrotInterpolant * 11.15 + float3(3, 3.5, 4)) * 0.5 + 0.5, 1);
    mandelbrotColor.rgb *= smoothstep(1, 0.97, mandelbrotInterpolant) * 4;
    
    return float4(0, 0, 0, 1) + lerp(mandelbrotColor, reflectionColor, smoothstep(0.88, 0.55, mandelbrotInterpolant) * 0.9);
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}