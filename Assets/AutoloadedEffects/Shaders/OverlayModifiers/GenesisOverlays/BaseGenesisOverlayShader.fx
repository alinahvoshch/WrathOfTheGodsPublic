sampler baseTexture : register(s0);
sampler seedlingTexture : register(s1);
sampler warpNoise : register(s2);
sampler lightTarget : register(s3);

float globalTime;
float lightInfluenceFactor;
float morphToGenesisInterpolant;
float2 pixelationFactor;
float2 textureSize0;
float2 textureSize1;
float2 zoom;
float2 screenArea;
float4 outlineColor;

bool IsActive(float2 coords)
{
    float4 data = tex2D(baseTexture, coords);
    return any(data.rgb) || data.a >= 1;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    coords = floor(coords / pixelationFactor) * pixelationFactor;
    
    float morphBump = sin(pow(morphToGenesisInterpolant, 0.7) * 3.141);
    
    float2 displacement = (tex2D(warpNoise, coords * 2).rg - 0.5) * morphBump * float2(0.04, 0.08);
    float crossfadeInterpolant = pow(smoothstep(0, 0.8, morphToGenesisInterpolant), 1.525);
    
    float4 seedlingColor = tex2D(seedlingTexture, coords + displacement);
    float4 genesisColor = tex2D(baseTexture, coords + displacement * 0.7);
    float4 color = lerp(seedlingColor, genesisColor, crossfadeInterpolant);
    color += morphBump * color.a * 2;
    
    float originalBrightness = dot(color.rgb, 0.333);
    float lightInfluence = 1 - tex2D(lightTarget, (position.xy / screenArea - 0.5) / zoom + 0.5).r;
    lightInfluence *= smoothstep(0.75, 0.46, originalBrightness);
    color.rgb = lerp(color.rgb, 0, lightInfluence * lightInfluenceFactor);
    
    float2 outlineOffset = 2 / textureSize0;
    bool outline = (!IsActive(coords + float2(-outlineOffset.x, 0)) ||
                   !IsActive(coords + float2(outlineOffset.x, 0)) ||
                   !IsActive(coords + float2(0, -outlineOffset.y)) ||
                   !IsActive(coords + float2(0, outlineOffset.y))) && any(color.rgb);
    
    return color * sampleColor + outline * outlineColor * pow(color.a, 1.5);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}