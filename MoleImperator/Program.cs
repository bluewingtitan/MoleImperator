using MoleImperator;
using PuppeteerSharp;

await ImperatorHelpers.EnsureBrowserAvailable();

// Account to use
var credentials = TestAccounts.MoleImp1;

while (true)
{
    var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = false,
        DefaultViewport = new ViewPortOptions
        {
            Width = 1920,
            Height = 1080,
            IsLandscape = true,
            IsMobile = false,
        },
        Args = new []{"--start-maximized"}
    
    });
    
    var session = new ImperatorSession(browser);
    await session.LogIn(credentials);

    await session.HarvestAll();
    var amount = session.GetFreeTileCount();
    await session.Plant(PlantType.Carrot, amount/2);
    await session.Plant(PlantType.Salad, amount/2);


    await Task.Delay(1000 * 60); 
    await browser.CloseAsync();

    await Task.Delay(1000 * 60 * 13);
}
