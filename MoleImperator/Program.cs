using MoleImperator;
using PuppeteerSharp;

await ImperatorHelpers.EnsureBrowserAvailable();

// Account to use
var credentials = TestAccounts.MoleImp1;

var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = false,
    DefaultViewport = new ViewPortOptions
    {
        Width = 1920,
        Height = 1080,
        IsLandscape = true,
        IsMobile = false,
    }
    
});

var session = new ImperatorSession(browser);
await session.LogIn(credentials);
Console.WriteLine(session.IsLoggedIn);