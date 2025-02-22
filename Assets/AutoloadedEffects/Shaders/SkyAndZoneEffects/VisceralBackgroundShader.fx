sampler vortexTexture : register(s1);
sampler flowTexture : register(s2);

float darkening;
float time;
float gradientCount;
float2 center;
float4 gradient[4];

float4 PaletteLerp(float interpolant)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float2 Field(float2 coords)
{
    // Use polar coordinates to sample a swirling, whirlpool-like vector field.
    float2 offset = (coords - center) * float2(0.95, 1.8);
    float distance = length(offset);
    float2 polar = float2(distance, atan2(offset.y, offset.x) * 0.159155 + 0.5);
    polar.x += time * 0.11;
    polar.y += time * -0.45 - distance * 8;
    
    // Use the aforementioned polar coordinates to calculate a direction angle.
    float angle = tex2D(vortexTexture, polar) * 7;
    
    // Calculate a fade variable. This makes the ends of the whirlpool dissipate and creates a hole in the center.
    float centerFade = smoothstep(0.025, 0.143, distance);
    float edgeFade = smoothstep(0.22, 0.17, distance - angle * 0.05);
    float fade = centerFade * edgeFade * 0.15;
    
    // Combine everything together.
    return (float2(cos(angle), -sin(angle)) + (angle - 3.141) * -0.1) * fade;
}

float2 IntegrateFlow(float2 coords, float t)
{
    // Simulate flow via an Euler-method integration technique.
    t *= 0.2;
    for (int i = 0; i < 5; i++)
        coords += t * Field(coords);
    
    return coords;
}

float CalculateLocalColorInterpolant(float2 coords)
{
    return tex2D(flowTexture, coords * 1.5);
}

float4 CalculateColor(float2 coords)
{
    // Calculate flow properties.
    // To allow for seamless time transitions, two flow values are calculated and then interpolated between, with the latter being slightly offset in time compared to the former.
    float flowTimeA = frac(time * 0.4);
    float flowTimeB = frac(flowTimeA + 0.5);
    float2 flowPositionA = IntegrateFlow(coords, flowTimeA);
    float2 flowPositionB = IntegrateFlow(coords, flowTimeB);
    float4 colorInterpolantA = CalculateLocalColorInterpolant(flowPositionA);
    float4 colorInterpolantB = CalculateLocalColorInterpolant(flowPositionB);
    
    // Combine two flow values together via interpolation in accordance with the flow time.
    float interpolant = abs(flowTimeA - 0.5) * 2;
    float effectiveSpeed = lerp(distance(coords, flowPositionA), distance(coords, flowPositionB), interpolant);
    float opacity = smoothstep(0, 0.09, effectiveSpeed);
    float colorInterpolant = lerp(colorInterpolantA, colorInterpolantB, interpolant) * opacity;
    
    // Use the result on a gradient-mapping to convert everything into blood vortex colors.
    return PaletteLerp(colorInterpolant) * float4(darkening, darkening, darkening, 1);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = float4(0, 0, 0, 1) + CalculateColor(coords);
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}