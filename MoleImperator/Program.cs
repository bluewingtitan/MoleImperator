using MoleImperator;
using PuppeteerSharp;

var fetcher = new BrowserFetcher(new BrowserFetcherOptions
{
    Product = Product.Chrome,
});
await fetcher.DownloadAsync();

var credentials = new ImperatorCredentials(5, "bluebloods", "j$AGntY6!7xkYwi^6Ch8Pcxuzi");

var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = false,
    
});

var session = new ImperatorSession(browser);
await session.LogIn(credentials);
