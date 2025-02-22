sampler2D baseTexture : register(s0);

bool fadeAtCenter;
bool invertColor;
float globalTime;
float scale;
float2 textureSize;

float4 Gaussian(float2 coords, float4 sampleColor, float2 blurOffset)
{ 
    // Sample the texture multiple times in radial directions and average the results to give the blur.
    float4 color = 0;
    color += tex2D(baseTexture, coords + float2(1, 0) * blurOffset) * sampleColor;
    color += tex2D(baseTexture, coords + float2(0.866025, 0.5) * blurOffset) * sampleColor;
    color += tex2D(baseTexture, coords + float2(0.5, 0.866025) * blurOffset) * sampleColor;
    color += tex2D(baseTexture, coords + float2(0, 1) * blurOffset) * sampleColor;
    
    color += tex2D(baseTexture, coords + float2(-0.5, 0.866025) * blurOffset) * sampleColor;
    color += tex2D(baseTexture, coords + float2(-0.866025, 0.5) * blurOffset) * sampleColor;
    color += tex2D(baseTexture, coords + float2(-1, 0) * blurOffset) * sampleColor;
    
    color += tex2D(baseTexture, coords + float2(-0.866025, -0.5) * blurOffset) * sampleColor;
    color += tex2D(baseTexture, coords + float2(-0.5, -0.866025) * blurOffset) * sampleColor;
    color += tex2D(baseTexture, coords + float2(0, -1) * blurOffset) * sampleColor;
    
    color += tex2D(baseTexture, coords + float2(0.5, -0.866025) * blurOffset) * sampleColor;
    color += tex2D(baseTexture, coords + float2(0.866025, -0.5) * blurOffset) * sampleColor;
    float4 invertedColor = float4(1 - color.rgb, 1) * color.a;
    
    return lerp(color, invertedColor, invertColor) * 0.08333333;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate the base color, applying a harsh darkening effect.
    float4 color = tex2D(baseTexture, coords) * float4(0.03, 0.03, 0.03, 1);
    float4 invertedColor = float4(1 - color.rgb, 1) * color.a;
    color = lerp(color, invertedColor, invertColor);
    
    // Blur the texture apply and make it red.
    float redBlur = Gaussian(coords, 1, 5 / textureSize) * 2;
    float4 red = redBlur * (1 - color.a) * float4(1.25, 0, 0, 1);
    
    // Fade in the center if necessary. This is used to ensure arms don't appear directly on top of the Avatar's rift.
    float centerFade = fadeAtCenter ? smoothstep(0.067, 0.092, distance(coords, 0.5) / scale) : 1;
    
    // Combine colors.
    return (color + red) * sampleColor * centerFade;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}