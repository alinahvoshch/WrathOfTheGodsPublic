sampler baseTexture : register(s0);
sampler invertDistanceNoiseTexture : register(s1);
sampler dissolveNoiseTexture : register(s2);
sampler dissolveDetailNoiseTexture : register(s3);

float globalTime;
float invertAnimationCompletion;
float blackDissolveInterpolant;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate polar coordinates.
    float4 color = tex2D(baseTexture, coords);
    float distanceFromCenter = distance(coords, 0.5);
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 3.141 + 0.5, distanceFromCenter);
    polar.y += globalTime * 0.1;
   
    float invertInterpolantNoise = tex2D(invertDistanceNoiseTexture, polar * 2) * 0.05;
    float invertInterpolant = smoothstep(0.95, 1, distanceFromCenter + invertAnimationCompletion + invertInterpolantNoise);
    
    // Calculate the inverse color. This will be interpolated towards based on the invertInterpolant values above.
    // This color is given a bit of extra brightness at the edges, to create a slightly "dreamy" feel to it.
    float3 inverseColor = 1 - color.rgb;
    inverseColor += pow(distanceFromCenter + invertInterpolantNoise, 2);
    
    // Interpolate colors.
    color.rgb = lerp(color.rgb, inverseColor, invertInterpolant);
    
    // Make colors dissolve based on the blackDissolveInterpolant value.
    float localBlackDissolveInterpolant = saturate(blackDissolveInterpolant + tex2D(dissolveDetailNoiseTexture, coords * 0.93) * 0.34);
    float dissolvePower = tex2D(dissolveNoiseTexture, coords * 0.72) * 3 + 1;
    float dissolveNoise = (tex2D(dissolveNoiseTexture, coords * 0.6) + tex2D(dissolveNoiseTexture, coords * 1.32)) * 0.5;
    float blackInterpolant = smoothstep(0.77, 0.85, dissolveNoise * 0.77 + pow(blackDissolveInterpolant, dissolvePower) * 1.32);
    color.rgb = lerp(color.rgb, 0, blackInterpolant);
    
    return color * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}