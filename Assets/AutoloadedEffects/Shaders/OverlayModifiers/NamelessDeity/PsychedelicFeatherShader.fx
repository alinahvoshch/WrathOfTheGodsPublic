sampler baseTexture : register(s0);
sampler eyeMaskTexture : register(s1);
sampler noiseTexture : register(s2);
sampler psychedelicTexture : register(s3);

float localTime;
float pupilDilation;
float blurOffset;
float blurWeights[7];
float psychedelicInterpolant;
float2 pupilOffset;
float2 textureSize1;
float3 outerIrisColor;
float3 innerIrisColor;
float4 frame;

float4 CalculatePsychedelicColor(float2 coords, float4 originalColor)
{
    float4 additivePsychedelicColor = 0;
    for (int i = 0; i < 3; i++)
    {
        float distortion = additivePsychedelicColor.r * 0.25;
        float2 psychedelicCoords = coords * float2(i * 0.2 + 0.5, i * 0.1 + 1) * 0.4 + distortion + float2(0, localTime * -0.3 / (i + 1));
        additivePsychedelicColor += tex2D(psychedelicTexture, psychedelicCoords);
    }
    
    float4 finalPsychedelicColor = additivePsychedelicColor * psychedelicInterpolant * originalColor.a;
    return originalColor + finalPsychedelicColor;
}

float4 CalculateEyeColor(float2 coords, float2 framedCoords, float4 originalColor)
{
    float2 eyeStart = float2(0.5, 0.225) + pupilOffset;
    float2 polar = float2(atan2(eyeStart.y - framedCoords.y, eyeStart.x - framedCoords.x) / 6.283 + 0.5, distance((framedCoords - eyeStart) * float2(1, frame.w / frame.z) + eyeStart, eyeStart));
    
    // Outer iris.
    float3 eyeColor = outerIrisColor;
    
    // Inner iris.
    eyeColor = lerp(eyeColor, innerIrisColor, smoothstep(0.22, 0.1, polar.y));
    
    // Cornea darkening.
    float corneaDarkenInterpolant = smoothstep(0.67, 0, tex2D(noiseTexture, polar * float2(5, 0.5))) * 0.7;
    eyeColor = lerp(eyeColor, 0, corneaDarkenInterpolant);
    
    // Cornea brightening.
    float corneaBrightenInterpolant = smoothstep(0.3, 1, tex2D(noiseTexture, polar * float2(6, 0.9)));
    eyeColor = lerp(eyeColor, 1, corneaBrightenInterpolant);
    
    // Highlight.
    float highlightNoise = tex2D(noiseTexture, coords * 1.7);
    float2 highlightPosition = eyeStart + float2(0.11, -0.04);
    eyeColor += exp((distance(coords, highlightPosition) - highlightNoise * 0.05) * -16);
    
    // Pupil.
    eyeColor = lerp(eyeColor, 0, smoothstep(0.08, 0.07, polar.y / pupilDilation));
    
    float eyeOpacity = tex2D(eyeMaskTexture, coords).r;
    return lerp(originalColor, float4(eyeColor, 1), eyeOpacity);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 framedCoords = (coords * textureSize1 - frame.xy) / frame.zw;
    
    float4 color = 0;
    float psychedelicOffset = (tex2D(psychedelicTexture, coords * 0.76 + localTime * 0.1) - 0.5) * saturate(psychedelicInterpolant) * 0.09;
    for (int i = 0; i < 7; i++)
    {
        float2 offset = float2(0, blurOffset * i) + psychedelicOffset;
        color += tex2D(baseTexture, coords - offset) * blurWeights[i];
        color += tex2D(baseTexture, coords + offset) * blurWeights[i];
    }
    color = CalculatePsychedelicColor(coords, color);
    color = CalculateEyeColor(coords, framedCoords, color);
    
    return color * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}