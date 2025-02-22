sampler baseTexture : register(s0);

float2 lineCenter;
float2 lineDirection;

float SignedDistanceToLine(float2 p, float2 linePoint, float2 lineDirection)
{
    return dot(lineDirection, p - linePoint);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float distanceFromLine = SignedDistanceToLine(position.xy, lineCenter, lineDirection);
    float opacity = smoothstep(0, 50, distanceFromLine);
    
    float4 color = tex2D(baseTexture, coords) * sampleColor;
    return color * opacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}