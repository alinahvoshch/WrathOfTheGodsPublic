sampler baseTexture : register(s0);
sampler bloodTexture : register(s1);
sampler myDistortioninator : register(s2);

float globalTime;
float2 screenPosition;
float2 targetSize;

float TopInterpolant(float2 coords)
{
    float result = 0;
    for (int i = 0; i < 25; i++)
        result += any(tex2D(baseTexture, coords - float2(0, i * 0.0022)));
    
    return 1 - result * 0.04;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate the world UV coordinates.
    float2 worldStableCoords = coords - screenPosition / targetSize;
    
    // Calculate the base color and its brightness, for later.
    float4 color = tex2D(baseTexture, coords);
    float brightness = dot(color.rgb, float3(0.3, 0.6, 0.1));
   
    // Calculate how much blood should be added based on how close to the top of the liquid the given pixel is.
    float bloodInterpolant = TopInterpolant(coords);
    
    // Calculate the blood color, applying the noise-induced offsets.
    float2 distortionOffset = (tex2D(myDistortioninator, worldStableCoords * 3.5 + float2(globalTime * 0.08, 0)).r - 0.5) * float2(0.1, 0.04);
    float2 bloodCoords = worldStableCoords * 8 + float2(globalTime * 0.1, 0) + distortionOffset;
    float4 bloodColor = (tex2D(bloodTexture, bloodCoords * 1.05) + tex2D(bloodTexture, bloodCoords * 0.7 + float2(globalTime * -0.045, 0))) * 0.5;
    
    // Combine things together.
    color = lerp(color, bloodColor * brightness * 9, bloodInterpolant);    
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}