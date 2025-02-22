sampler noiseTexture : register(s1);
sampler nebulaTextureA : register(s2);
sampler nebulaTextureB : register(s3);

float nebulaColorExponent;
float nebulaColorIntensity;
float globalTime;
float upscaleFactor;
float2 viewOffset;
float3 nebulaColorA;
float3 nebulaColorB;
float3 nebulaColorC;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords -= viewOffset;
    
    float backgroundNoise = tex2D(noiseTexture, coords * 10) * tex2D(noiseTexture, coords * 6) * tex2D(noiseTexture, coords * 3);
    float3 baseColor = sampleColor.rgb * lerp(0.5, 2, backgroundNoise);
    
    float redNebulaInterpolant = tex2D(nebulaTextureA, coords * 1.1) * tex2D(nebulaTextureB, coords * 2.1) * tex2D(noiseTexture, coords * 1.1);
    float blueNebulaInterpolant = tex2D(nebulaTextureA, coords * 1.9) * tex2D(nebulaTextureB, coords * 1.2) * tex2D(noiseTexture, coords * 1.1) * (0.3 + redNebulaInterpolant * 1.5);
    float greenNebulaInterpolant = tex2D(nebulaTextureA, coords * 1.5) * tex2D(nebulaTextureB, coords * 1.8) * tex2D(noiseTexture, coords * 1.21);
    float3 nebulaColor = 0;
    nebulaColor += nebulaColorA * smoothstep(0.05, 0.25, redNebulaInterpolant) * 0.15;
    nebulaColor += nebulaColorB * smoothstep(0.05, 0.2, blueNebulaInterpolant) * 0.35;
    nebulaColor += nebulaColorC * smoothstep(0, 0.2, greenNebulaInterpolant) * pow(blueNebulaInterpolant, 1.75) * 8;
    nebulaColor = pow(nebulaColor, nebulaColorExponent) * nebulaColorIntensity;
    
    return float4(baseColor + nebulaColor, 1) * sampleColor.a;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
