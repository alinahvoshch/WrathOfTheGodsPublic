sampler baseTexture : register(s0);

// Refer to the screen shader's C# code to find out more about this. It's far more efficient to precompute it on the CPU than to have
// the matrix remade for every single pixel on the screen based on an input float.
float4x4 contrastMatrix;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    return mul(tex2D(baseTexture, coords), contrastMatrix);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}