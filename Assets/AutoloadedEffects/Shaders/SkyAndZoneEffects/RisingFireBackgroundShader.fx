sampler baseTexture : register(s0);
sampler fireTexture : register(s1);
sampler offsetNoise : register(s2);
sampler nonuniformarityNoise : register(s3);

float time;
float bottomGlowIntensity;
float gradientCount;
float horizontalSquish;
float3 gradient[8];

/*
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

        float hueOffset = 0f;
        Vector3[] palette = new Vector3[]
        {
            Color.Wheat.HueShift(hueOffset).ToVector3(),
            Color.Orange.HueShift(hueOffset).ToVector3(),
            Color.DarkGray.ToVector3(),
        };

        ManagedShader fireShader = ShaderManager.GetShader("NoxusBoss.RisingFireBackgroundShader");
        fireShader.TrySetParameter("bottomGlowIntensity", 0.2f);
        fireShader.TrySetParameter("horizontalSquish", 1.81f);
        fireShader.TrySetParameter("gradient", palette);
        fireShader.TrySetParameter("gradientCount", palette.Length);
        fireShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 1.5f);
        fireShader.SetTexture(FireNoiseB, 1, SamplerState.LinearWrap);
        fireShader.SetTexture(FireNoiseB, 2, SamplerState.LinearWrap);
        fireShader.SetTexture(WavyBlotchNoise, 3, SamplerState.LinearWrap);
        fireShader.Apply();

        Main.spriteBatch.Draw(WhitePixel, new Rectangle(0, 0, Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height), Color.White);
*/

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(frac(interpolant) * gradientCount, 0, gradientCount - 1);
    int endIndex = (startIndex + 1) % gradientCount;
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}

float2 CalculateFireLayer(float2 coords, float zoom, float height)
{
    return tex2D(fireTexture, coords * zoom) * smoothstep(1 - height, 1, coords.y);
}

float ScreenBlend(float base, float screen)
{
    return 1 - (1 - base) * (1 - screen);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    // Apply variable offsets based on noise to make the fire less uniform.
    float nonuniformityNoise = tex2D(nonuniformarityNoise, coords * float2(0.4, 0.03) + time * 0.02) + tex2D(nonuniformarityNoise, coords * float2(0.5, 0.02) + time * 0.035);
    coords.x += (nonuniformityNoise - 0.5) * (1 - coords.y) * 0.03;
    coords.y -= nonuniformityNoise * 0.2 + 0.05;
    
    // Calculate self-affecting noise in succesion, similar to FBM.
    // This will yield noise values that are moderately distorted and quite detailed, which are ideal conditions for heat distortion effects.
    float noise = 0;
    for (int i = 2; i >= 0; i--)
    {
        float scrollSpeed = i * 0.3 + 0.3;
        float horizontalSway = coords.y * lerp(0.7, 1, noise) * cos(coords.x * 4 + time * 1.05) * 0.2;
        float verticalSway = abs(horizontalSway) * -0.1;
        
        noise = tex2D(offsetNoise, coords * (i * 0.5 + 1) * float2(horizontalSquish, 1) + float2(i * 0.25 + horizontalSway, time * scrollSpeed + verticalSway) + noise * 0.02);
    }
    
    // Vertically offset the scene based on the aforementioned noise value.
    // The intensity of this offset decreases closer to the bottom of the scene.
    coords.y += noise * smoothstep(1.5, 0, coords.y) * 0.24;
    
    // Calculate three separate layers of fire at different frequencies and different heights.
    // Successive layers are modified by the previous layer if applicable, for extra detail.
    float fireA = CalculateFireLayer(coords, 0.5, 0.45);
    float fireB = CalculateFireLayer(coords + fireA * 0.2, 1.2, 0.7);
    float fireC = CalculateFireLayer(coords + fireB * 0.2 + 0.05, 2, 0.95);
    
    // Combine the layers of fire together with a screen blend mode. This looks a touch better than additive blending.
    // Furthermore, additively whiten the bottom of the scene, to create a defined bottom of the flame.
    float bottom = smoothstep(0.4, 1, coords.y) * bottomGlowIntensity;
    float fire = ScreenBlend(ScreenBlend(fireA, fireB), fireC) * 0.5 + bottom;
    
    // Use the combined layers of fire, along with a linear palette interpolation, to calculate the final result.
    float3 baseColor = PaletteLerp(noise * 0.07 - fire * 0.5 + (1 - coords.y - fireB * 0.5) * 0.8);
    float3 result = fire / (1.11 - baseColor);
    
    return float4(result, 1);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}