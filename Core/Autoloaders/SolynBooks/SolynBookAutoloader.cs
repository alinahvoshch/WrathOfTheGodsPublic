using Terraria.ModLoader;

namespace NoxusBoss.Core.Autoloaders.SolynBooks;

public static class SolynBookAutoloader
{
    /// <summary>
    /// The set of all books registered, for easy access.
    /// </summary>
    public static readonly Dictionary<string, AutoloadableSolynBook> Books = [];

    /// <summary>
    /// Creates a new book instance.
    /// </summary>
    public static AutoloadableSolynBook Create(Mod mod, LoadableBookData data)
    {
        AutoloadableSolynBook book = new AutoloadableSolynBook(data);
        mod.AddContent(book);

        Books[book.Name] = book;

        return book;
    }
}
