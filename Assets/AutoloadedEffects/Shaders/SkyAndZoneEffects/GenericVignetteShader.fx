float vignetteEdgeStart;
float vignetteEdgeEnd;
float globalTime;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float distanceFromCenter = distance(coords, 0.5);
    return smoothstep(vignetteEdgeStart, vignetteEdgeEnd, distanceFromCenter) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
