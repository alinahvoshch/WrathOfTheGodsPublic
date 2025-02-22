using Terraria;

namespace NoxusBoss.Content.Tiles.GenesisComponents;

public struct GenesisGrassMergeData : ITileData
{
    private byte leftConversionData;

    private byte rightConversionData;

    /// <summary>
    /// How much the grass has converted on its left side.
    /// </summary>
    public float LeftConversionInterpolant
    {
        readonly get => leftConversionData / 255f;
        set => leftConversionData = (byte)(Saturate(value) * 255f);
    }

    /// <summary>
    /// How much the grass has converted on its right side.
    /// </summary>
    public float RightConversionInterpolant
    {
        readonly get => rightConversionData / 255f;
        set => rightConversionData = (byte)(Saturate(value) * 255f);
    }

    /// <summary>
    /// Whether this grass is unconverted or not.
    /// </summary>
    public readonly bool Unconverted => leftConversionData == 0 && rightConversionData == 0;

    /// <summary>
    /// Clears the packed binary data for this grass, effectively zeroing out the conversion interpolants.
    /// </summary>
    public void Clear()
    {
        leftConversionData = 0;
        rightConversionData = 0;
    }
}
