using NoxusBoss.Content.Items.LoreItems;
using ReLogic.Graphics;
using Terraria;
using Terraria.Utilities;

namespace NoxusBoss.Core.DataStructures;

public class ContextFreeGrammar(int seed, Dictionary<string, List<string>> grammar)
{
    protected Dictionary<string, List<string>> grammar = grammar;

    protected UnifiedRandom rng = new UnifiedRandom(seed);

    protected int recursionLimitCounter;

    protected readonly int seed = seed;

    protected string InternalGenerate(string symbol)
    {
        recursionLimitCounter++;

        // Terminate if the grammer does not contain the given symbol.
        if (!grammar.TryGetValue(symbol, out List<string>? production) || recursionLimitCounter >= 50)
            return symbol;

        string randomToken = rng.Next(production);
        string[] newSymbols = randomToken.Split(' ');

        // Recursively combine tokens until the entire result is populated with custom words.
        return string.Join(" ", newSymbols.Select(InternalGenerate));
    }

    public string GenerateSentence(DynamicSpriteFont font)
    {
        // Generate the base sentence.
        string sentence;
        float maxLengthDeviation = NamelessDeityLoreManager.UseTrollingMode ? float.MaxValue : 30f;
        do
        {
            recursionLimitCounter = 0;
            sentence = InternalGenerate("SENTENCE");
            recursionLimitCounter = 0;

            // Replace "a blahblahblah" with "an blahblahblah" if the next word starts with a vowel.
            sentence = $" {sentence}";
            sentence = sentence.Replace(" a a", " an a").Replace(" a e", " an e").Replace(" a i", " an i").Replace(" a o", " an o").Replace(" a u", " an u");
            sentence = sentence.Replace(" a A", " an A").Replace(" a E", " an E").Replace(" a I", " an I").Replace(" a O", " an O").Replace(" a U", " an U");
            sentence = sentence[1..];

            // Ensure that the first character is capitalized.
            sentence = string.Concat(sentence.ToUpper()[0].ToString(), sentence.AsSpan(1));

            // Ogscule.
            sentence = sentence.Replace("the ogscule", "ogscule").Replace("a ogscule", "ogscule");

            // Replace spaces as necessary if the language doesn't use them.
            sentence = sentence.Replace(" ", LoreNamelessDeity.textSpacer);

            // Add a period at the end.
            sentence += ".";

            if (NamelessDeityLoreManager.UseTrollingMode)
                break;
        }
        while (Distance(font.MeasureString(sentence).X, 820f) >= maxLengthDeviation);

        // Very occasionally replace text with customized easter egg text.
        if (rng.NextBool(NamelessDeityLoreManager.EasterEggLineChance))
            return NamelessDeityLoreManager.EasterEggLine;

        return sentence;
    }

    public void RegenerateRNG() => rng = new UnifiedRandom(seed);
}
