sampler screenTexture : register(s0);
sampler psychedelicTexture : register(s1);

float globalTime;
float lifetimeRatios[15];
float maxRadii[15];
float2 positions[15];

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 baseCoords = coords;
    float psychedelicColorInfluence = 0;
    
    for (int i = 0; i < 15; i++)
    {
        float lifetimeRatio = lifetimeRatios[i];
        float radius = maxRadii[i] * lifetimeRatio;
        float distanceFromPosition = distance(position.xy, positions[i]);
        float distanceFromEdge = distance(distanceFromPosition, radius);
        float intensity = smoothstep(0, 0.4, lifetimeRatio) * smoothstep(1, 0.4, lifetimeRatio) * exp(distanceFromEdge * -0.1);
        float2 directionFromDistortion = normalize(position.xy - positions[i]);
        
        coords += float2(directionFromDistortion.y, directionFromDistortion.x) * intensity * 0.1;
        psychedelicColorInfluence += intensity;
    }
    
    float4 baseColor = tex2D(screenTexture, coords);
    float4 psychedelicColor = tex2D(psychedelicTexture, baseCoords * 2 + coords * 3);
    return baseColor + psychedelicColor * psychedelicColorInfluence;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
