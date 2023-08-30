using MoleImperator;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;

await ImperatorHelpers.EnsureBrowserAvailable();
ImperatorHelpers.KillOldChrome();

// Account to use
var credentials = TestAccounts.MoleImp1;
var extra = new PuppeteerExtra(); 

// Use stealth plugin
extra.Use(new StealthPlugin());

var browser = await extra.LaunchAsync(new LaunchOptions
{
    Headless = true,
    DefaultViewport = new ViewPortOptions
    {
        Width = 1280,
        Height = 720,
        DeviceScaleFactor = 0.5,
        IsLandscape = true,
        IsMobile = false,
    },
    Args = new []{"--window-size=1920,1080", @$"--user-agent=""{ImperatorSession.USER_AGENT}"""}
    
});

AppDomain.CurrentDomain.ProcessExit += async (_, _) => await browser.CloseAsync();

var retryCount = 0;
var i = 0;
while (true)
{
    Console.WriteLine("New iteration starts now.");

    BrowserContext? context = null;
    
    try
    {
        context = await browser.CreateIncognitoBrowserContextAsync();
        var session = new ImperatorSession(context);
        await session.LogIn(credentials);
        
        Console.WriteLine("Logged in.");

        await session.HarvestAll();
        Console.WriteLine("Harvested.");
        
        var amount = session.GetPlantableTileCount();
    
        var toPlant = new List<PlantType>()
        {
            PlantType.Salad,
            PlantType.Carrot,
            PlantType.Carrot,
            PlantType.Cucumber,
            PlantType.Radish,
            PlantType.Tomato,
            PlantType.Strawberry,
            PlantType.Spinach,
            PlantType.Onion,
        };

        var toPlantAmount = amount / toPlant.Count;

        foreach (var type in toPlant)
        {
            await session.EnsureIsPlanted(type, toPlantAmount);
        }

        var leftOver = session.GetPlantableTileCount();
        if (leftOver > 0)
        {
            await session.Plant(toPlant[0], leftOver);
        }
        
        Console.WriteLine("Planted.");


        await Task.Delay(1000 * 60);
    }
    catch (Exception e)
    {
        // retry.

        if (context != null)
        {
            await context.CloseAsync();
        }
        
        Console.WriteLine($"Exception occured while processing Iteration: {e.GetType().FullName}.");

        if (retryCount > 30)
        {
            Console.WriteLine("Too many exceptions. Make sure the used account is valid and your internet connection is stable.");
            await browser.CloseAsync();
            throw;
        }
        
        retryCount++;
        Console.WriteLine($"Retry {retryCount}/30 in 30 seconds...");
        await Task.Delay(30 * 1000);
        continue;
    }
    retryCount = 0;
    i++;

    await context.CloseAsync();

    Console.Clear();
    Console.WriteLine($"Next iteration ({i+1}) will happen at {DateTime.Now + TimeSpan.FromMinutes(13)}");
    await Task.Delay(1000 * 60 * 13);
}

await browser.CloseAsync();