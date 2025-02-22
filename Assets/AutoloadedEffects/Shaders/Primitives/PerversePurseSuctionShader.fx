sampler baseTexture : register(s1);

float localTime;
float vanishInterpolant;
float rotationSwerve;
float2 pixelationDetail;
float3 minorSuctionColor;
float3 majorSuctionColor;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

float2 RotatedBy(float2 v, float theta)
{
    float c = cos(theta);
    float s = sin(theta);

    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    
    float undulation = sin(input.TextureCoordinates.y * 6.283 - localTime * 10) * QuadraticBump(input.TextureCoordinates.y) * 0.1;
    
    input.Position.x += undulation;
    input.Position.x *= lerp(0.05, 1, pow(smoothstep(0, 1, input.TextureCoordinates.y), 0.7));
    input.Position.x *= lerp(0, 1, smoothstep(0, 0.05, input.TextureCoordinates.y));
    input.Position.xy = RotatedBy(input.Position.xy, input.TextureCoordinates.y * rotationSwerve);
    
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = floor(input.TextureCoordinates * pixelationDetail) / pixelationDetail;
    float4 color = input.Color;
    
    float cosAngle = coords.x;
    coords.x = acos(cosAngle) / 3.141;
    
    // Calculate how much the suction noise values should fade away.
    // The greater this value is, the more of a tendency the suction bits will have to dissapear.
    float fadeOut = smoothstep(0.7, 1, coords.y) + vanishInterpolant;
    
    // Calculate the first suction color.
    float suctionNoiseA = tex2D(baseTexture, coords * float2(-1, 0.5) + float2(2, 1) * localTime + float2(coords.y * -0.72, 0));
    float suctionPartA = smoothstep(0.5, 0.8, suctionNoiseA - fadeOut);
    float4 suctionColorA = float4(minorSuctionColor, 1) * suctionPartA;
    
    // Calculate the second suction color.
    // In order to give the impression of depth, this spins in the opposite direction to the first suction value.
    float suctionNoiseB = tex2D(baseTexture, coords * float2(0.7, 0.25) + float2(1.25, 1) * localTime + float2(coords.y * -0.6, 0));
    suctionNoiseB += tex2D(baseTexture, coords + float2(0, localTime)) * pow(1 - coords.y, 2) * 1.5;
    suctionNoiseB += smoothstep(0.05, 0, coords.y);
    float suctionPartB = smoothstep(0.8, 0.9, suctionNoiseB - fadeOut);
    float4 suctionColorB = float4(majorSuctionColor, 1) * suctionPartB;
    
    // Combine the suction colors such that the brightest color takes priority, and then darken the results near the start of the effect.
    float darkening = smoothstep(-0.2, 0.7, coords.y);
    float4 suctionColor = max(suctionColorA, suctionColorB) * float4(darkening, darkening, darkening, 1);
    
    // Make edges of the suction fade out.
    float edgeOpacity = smoothstep(1, 0.7, abs(cosAngle));
    
    return suctionColor * edgeOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
