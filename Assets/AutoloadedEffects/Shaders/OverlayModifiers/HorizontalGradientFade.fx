sampler baseTexture : register(s0);

float leftFadeStart;
float leftFadeEnd;
float rightFadeStart;
float rightFadeEnd;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float leftFade = smoothstep(leftFadeStart, leftFadeEnd, coords.x);
    float rightFade = smoothstep(rightFadeEnd, rightFadeStart, coords.x);
    
    float4 result = tex2D(baseTexture, coords);
    return result * sampleColor * leftFade * rightFade;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}