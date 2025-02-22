sampler baseTexture : register(s0);

float globalTime;
float pixelation;
float lineDistance;
float2 textSize;
float2 fontOrientation;
float2 fontCenter;
float4 secondaryColor;

float SignedDistanceToLine(float2 p, float2 linePoint, float2 lineDirection)
{
    return dot(lineDirection, p - linePoint);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float distanceFromCenter = abs(SignedDistanceToLine(position.xy, fontCenter, fontOrientation));
    sampleColor = lerp(sampleColor, secondaryColor * sampleColor.a, distanceFromCenter <= lineDistance + sin(position.x * 0.032) * 2);
    
    float pixelationFactor = pixelation / textSize;
    coords = floor(coords / pixelationFactor) * pixelationFactor;
    
    float4 baseColor = tex2D(baseTexture, coords) * sampleColor;
    return baseColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}