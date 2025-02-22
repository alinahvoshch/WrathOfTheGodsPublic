sampler baseTexture : register(s0);
sampler maskTexture : register(s1);
sampler noiseTextureA : register(s2);
sampler noiseTextureB : register(s3);

float globalTime;
float2 textureSize1;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 pixelationFactor = 1.5 / textureSize1;
    coords = floor(coords / pixelationFactor) * pixelationFactor;
        
    float4 maskData = tex2D(maskTexture, coords);
    float glowDistortion = smoothstep(0.6, 0.2, coords.x);
    
    float2 glowCoordsA = coords * 0.45 + float2(globalTime * -0.03, 0);
    float glowNoiseA = tex2D(noiseTextureA, glowCoordsA);
    float2 glowCoordsB = coords * 1.24 + float2(globalTime * -0.05, 0);
    float glowNoiseB = tex2D(noiseTextureA, glowCoordsB);
    float circleGlowNoise = sqrt(glowNoiseA * glowNoiseB);
    
    float circleGlowShrink = tex2D(noiseTextureB, glowCoordsA * 1.1 + globalTime * 0.01);
    float circleGlow = 0.22 / (1 - circleGlowNoise);
    circleGlow = 0.22 / distance(circleGlow, lerp(0.9, 2, circleGlowShrink));
    circleGlow = smoothstep(0.3, 1, circleGlow);
    
    float wavyGlow = smoothstep(0.4, 0, tex2D(noiseTextureA, glowCoordsA * 2));
    
    float glow = lerp(circleGlow, wavyGlow, glowDistortion);
    glow = smoothstep(0, 0.9, glow);
    
    float glowOpacity = maskData.a * smoothstep(0.9, 0.7, coords.x);
    float4 glowColor = float4(0, 0.4, 1, 0) * glow + smoothstep(0.8, 1, glow) * 0.25;
    
    return tex2D(baseTexture, coords) + glowColor * glowOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}