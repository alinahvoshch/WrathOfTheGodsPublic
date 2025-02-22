namespace NoxusBoss.Core.CrossCompatibility.Inbound.WikiThis;

public interface IWikithisNameRedirect
{
    /// <summary>
    /// The name of the page that this content should redirect to.
    /// </summary>
    string RedirectPageName
    {
        get;
    }
}
