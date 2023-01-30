using System.Diagnostics;
using PuppeteerSharp;

namespace MoleImperator;

public static class ImperatorHelpers
{
    public static async Task RemovePopups(this Page p)
    {
        await p.AcceptCookieBanner();
        await p.AcceptDailyRewards();
        await p.CloseTutorial();
        await p.CloseHarvestPopUps();
    }

    public static async Task AcceptCookieBanner(this Page p)
    {
        await p.SelectAndDo(".cookiemon-btn-accept", e => e.ClickAsync());
    }

    public static async Task CloseTutorial(this Page p)
    {
        await p.SelectAndDo("#tutorialClose", e => e.ClickAsync());
        await p.EvaluateExpressionAsync("$('achievement').hide();");
        await p.EvaluateExpressionAsync("bonuspack.close();");
            
    }

    public static async Task CloseHarvestPopUps(this Page p)
    {
        await p.EvaluateExpressionAsync("basedialog.close()");
        await p.EvaluateExpressionAsync("$('ernte_log').hide();$('glock').hide();");
        //await p.SelectAndDo("img.link.closeBtn", e => e.ClickAsync());
    }

    public static async Task AcceptDailyRewards(this Page p)
    {
        await p.EvaluateExpressionAsync("dailyloginbonus.getReward()");
        await Task.Delay(1000);
        await p.EvaluateExpressionAsync("dailyloginbonus.close()");
    }

    public delegate Task ElementHandleAction(ElementHandle handle);
    public static async Task SelectAndDo(this Page p, string selector, ElementHandleAction action)
    {
        var elem = await p.QuerySelectorAsync(selector);

        if (elem != null && await elem.BoundingBoxAsync() != null)
        {
            await action(elem);
        }
    }
    public static async Task SelectAndDo(this ElementHandle p, string selector, ElementHandleAction action)
    {
        var elem = await p.QuerySelectorAsync(selector);

        if (elem != null && await elem.BoundingBoxAsync() != null)
        {
            await action(elem);
        }
    }

    public static async Task ScreenshotNow(this Page p)
    {
        await p.ScreenshotAsync(@$"D:\dev\MoleImperator\screenshots\{DateTime.Now.ToFileTimeUtc()}.png");
    }
    
    public static async Task ClearCookies(this Page p)
    {
        var cookies = await p.GetCookiesAsync();
        await p.DeleteCookieAsync(cookies);
    }

    public static async Task EnsureBrowserAvailable()
    {
        var fetcher = new BrowserFetcher(new BrowserFetcherOptions
        {
            Product = Product.Chrome,
        });
        Console.WriteLine("Potentially Downloading Browser. This might take a while.");
        fetcher.DownloadProgressChanged += (sender, args) => Console.WriteLine($"Download Progress: {args.ProgressPercentage}%");
        await fetcher.DownloadAsync();
        
        Console.Clear();
    }

    public static void KillOldChrome()
    {
        Console.WriteLine("This program kills all instances of chrome on startup. This is to clean browser-instances that sometimes get leaked by Puppeteer-Sharp.");
        foreach (var process in Process.GetProcessesByName("chrome"))
        {
            process.Kill();
        }
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