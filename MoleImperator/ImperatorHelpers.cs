using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace MoleImperator;

public static class ImperatorHelpers
{
    public static async Task RemovePopups(this IPage p)
    {
        await p.AcceptCookieBanner();
        await p.AcceptDailyRewards();
    }

    public static async Task AcceptCookieBanner(this IPage p)
    {
        await p.SelectAndDo(".cookiemon-btn-accept", e => e.ClickAsync(new ClickOptions{Delay = 10}));
    }

    public static async Task AcceptDailyRewards(this IPage p)
    {
        await p.EvaluateExpressionAsync("dailyloginbonus.getReward()");
        await Task.Delay(1000);
        await p.EvaluateExpressionAsync("dailyloginbonus.close()");
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


    public static async Task EnsureBrowserAvailable()
    {
        var fetcher = new BrowserFetcher(new BrowserFetcherOptions
        {
            Product = Product.Chrome,
        });
        await fetcher.DownloadAsync();
    }


    public static TimeSpan ParseTimeString(string input)
    {
        var parts = input.Split(":");

        if (parts.Length == 3)
        {
            return ParseTimeStringNoDays(parts);
        }

        if (parts.Length == 4)
        {
            return ParseTimeStringWithDays(parts);
        }

        throw new NotImplementedException($"Parsing of Time Strings with a month component not yet available: {input}");
    }

    private static TimeSpan ParseTimeStringNoDays(string[] parts)
    {
        int hours, minutes, seconds;

        if (int.TryParse(parts[0], out hours)
            && int.TryParse(parts[1], out minutes)
            && int.TryParse(parts[2], out seconds))
        {

            return TimeSpan.FromSeconds(seconds + 60*(minutes + 60*hours));
        }

        throw new ArgumentException($"Passed in bad time string: \"{parts[0]}:{parts[1]}:{parts[2]}\"");
    }private static TimeSpan ParseTimeStringWithDays(string[] parts)
    {
        int days, hours, minutes, seconds;

        if (int.TryParse(parts[0], out days)
            && int.TryParse(parts[1], out hours)
            && int.TryParse(parts[2], out minutes)
            && int.TryParse(parts[3], out seconds))
        {

            return TimeSpan.FromSeconds(seconds + 60*(minutes + 60*(hours + 24*days)));
        }

        throw new ArgumentException($"Passed in bad time string: \"{parts[0]}:{parts[1]}:{parts[2]}:{parts[3]}\"");
    }

}