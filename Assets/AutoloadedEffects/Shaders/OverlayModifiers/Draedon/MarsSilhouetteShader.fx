sampler baseTexture : register(s0);
sampler burnMarkTarget : register(s1);

float globalTime;
float silhouetteInterpolant;
float4 silhouetteColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float4 baseColor = tex2D(baseTexture, coords);
    float brightness = dot(baseColor.rgb, float3(0.3, 0.6, 0.1));
    float brightnessInfluence = smoothstep(0.7, 0.9, brightness) * smoothstep(0.45, 0.35, coords.y) * 0.7;
    float fadeToSilhouetteColor = saturate(pow(silhouetteInterpolant, 0.55) - brightnessInfluence);
    
    float silhouetteOpacity = baseColor.a * smoothstep(0.65, 0.55, coords.y);
    
    float4 color = lerp(baseColor, silhouetteColor * silhouetteOpacity, fadeToSilhouetteColor);
    
    color.rgb -= tex2D(burnMarkTarget, coords) * 0.4;
    
    return color * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}