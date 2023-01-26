using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace MoleImperator;

public static class ImperatorHelpers
{
    public static async Task RemovePopups(this IPage p)
    {
        await p.AcceptCookieBanner();
    }

    public static async Task AcceptCookieBanner(this IPage p)
    {
        await p.SelectAndDo(".cookiemon-btn-accept", e => e.ClickAsync(new ClickOptions{Delay = 10}));
    }

    public delegate Task ElementHandleAction(IElementHandle handle);
    public static async Task SelectAndDo(this IPage p, string selector, ElementHandleAction action)
    {
        var elem = await p.QuerySelectorAsync(selector);

        if (elem != null)
        {
            await action(elem);
        }
    }

}