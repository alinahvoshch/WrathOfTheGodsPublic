sampler baseTexture : register(s0);
sampler electicNoiseTexture : register(s1);

float globalTime;
float posterizationDetail;
float2 size;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Apply pixeliation.
    float2 pixelationFactor = 2 / size;
    coords = floor(coords / pixelationFactor) * pixelationFactor;
    
    // Calculate polar coordinates. Standard stuff.
    float distanceFromCenter = distance(coords, 0.5);
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distanceFromCenter);
    
    // Combine two polar-based noise values for use for the noise calculation below.
    float noise = tex2D(electicNoiseTexture, polar * 2 + float2(0, globalTime * -1.1)) + tex2D(electicNoiseTexture, polar + globalTime * float2(0.1, -0.5));
    
    // Calculate the glow of the color pixel.
    // This is influenced by noise to add spikiness to the shape of the white parts of the noise.
    float glow = 0.14 / max(0.0001, distanceFromCenter - noise * 0.1);
    float4 baseColor = saturate(tex2D(baseTexture, coords) * sampleColor * glow);
    
    // Apply posterization.
    baseColor = round(baseColor * posterizationDetail) / posterizationDetail;
    
    return baseColor * smoothstep(0.5, 0.2, distanceFromCenter);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}