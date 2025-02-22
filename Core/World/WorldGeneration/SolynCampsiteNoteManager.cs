using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Core.Graphics.UI.Books;
using NoxusBoss.Core.World.WorldSaving;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Core.World.WorldGeneration;

public class SolynCampsiteNoteManager : ModSystem
{
    /// <summary>
    /// Whether any players have received Solyn's handwritten note.
    /// </summary>
    public static bool HasReceivedNote
    {
        get;
        set;
    }

    /// <summary>
    /// Whether Solyn's note is inside of the tent.
    /// </summary>
    public static bool NoteIsInTent => !HasReceivedNote && BossDownedSaveSystem.HasDefeated<AvatarOfEmptiness>() && SolynBookExchangeRegistry.RedeemedAllBooks;

    public override void SaveWorldData(TagCompound tag) => tag[nameof(HasReceivedNote)] = HasReceivedNote;

    public override void LoadWorldData(TagCompound tag) => HasReceivedNote = tag.GetBool(nameof(HasReceivedNote));

    public override void NetSend(BinaryWriter writer) => writer.Write(HasReceivedNote);

    public override void NetReceive(BinaryReader reader) => HasReceivedNote = reader.ReadBoolean();
}
