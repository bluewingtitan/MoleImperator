using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace MoleImperator;

public class ImperatorSession
{

    public ImperatorSession(IBrowser browser)
    {
        _browser = browser;
    }

    private readonly IBrowser _browser;
    private IPage? _page = null;
    public DateTime ActivationTime { get; private set; } = DateTime.MinValue;
    public TimeSpan ActiveTime => DateTime.Now - ActivationTime;
    
    /// <summary>
    /// Molehill Empire invalidates sessions after 2h.
    /// </summary>
    public bool IsValid => ActiveTime.TotalHours < 2 && IsLoggedIn;
    
    public bool IsLoggedIn { get; private set; }

    public async Task LogIn(ImperatorCredentials credentials)
    {
        if (IsValid)
        {
            return;
        }

        if (_page != null)
        {
            await _page.CloseAsync();
            _page = null;
        }
        
        _page = await _browser.NewPageAsync();

        await _page.GoToAsync("https://www.wurzelimperium.de/");
        await Task.Delay(2000);

        await _page.AcceptCookieBanner();


        bool serverSelected = false;
        await _page.SelectAndDo("#login_server", async e =>
        {
            serverSelected = true;
            await e.SelectAsync($"server{credentials.Server}");
        });
        
        await Task.Delay(1000);
        
        bool loginTyped = false;
        await _page.SelectAndDo("#login_user", async e =>
        {
            loginTyped = true;
            
            await e.ClickAsync();
            await e.FocusAsync();
            await Task.Delay(1000);

            await e.TypeAsync(credentials.Login, new TypeOptions{Delay = 12});
        });

        await Task.Delay(1000);

        bool passwordTyped = false;        
        await _page.SelectAndDo("#login_pass", async e =>
        {
            passwordTyped = true;

            await e.ClickAsync();
            await e.FocusAsync();
            await Task.Delay(1000);

            await e.TypeAsync(credentials.Password, new TypeOptions { Delay = 15 });
        });

        if (loginTyped && passwordTyped && serverSelected)
        {
            await _page.SelectAndDo("#submitlogin", async e =>
            {
                await e.ClickAsync();
            });

            await _page.WaitForNavigationAsync(new NavigationOptions
            {
                Timeout = 120 * 1000
            });
            await Task.Delay(3000);
            
            if (_page.Url.Contains("main.php"))
            {
                IsLoggedIn = true;
                ActivationTime = DateTime.Now;

                await _page.RemovePopups();
                await ScanFields();
            }
        }
    }


    public async Task ScanFields()
    {
        await PerField(async (handle, page, tile) =>
        {
            await handle.HoverAsync();
            await Task.Delay(10);
            
            // get announcement field
            var plantDataBox = await page.QuerySelectorAsync("#sprcontent");
            if (plantDataBox == null)
            {
                throw new Exception($"Missing Tile with id {tile.TileId}. Is the correct site loaded?");
            }

            if (await plantDataBox.BoundingBoxAsync() == null)
            {
                //nothing planted here.
                tile.Update(PlantType.None, DateTime.MaxValue);
                return;
            }
            
            var typeElement = await plantDataBox.QuerySelectorAsync("b");
            if (typeElement == null || await typeElement.BoundingBoxAsync() == null)
            {
                //nothing planted here.
                tile.Update(PlantType.None, DateTime.MaxValue);
                return;
            }
            
            var name = await (await typeElement.GetPropertyAsync("innerText")).JsonValueAsync() as string;
            if (name == null)
            {
                //nothing planted here.
                tile.Update(PlantType.None, DateTime.MaxValue);
                return;
            }
            
            // TODO: Lookup type from string
            if (!PlantTypeData.Types.TryGetValue(name, out var type))
            {
                //nothing known planted here.
                tile.Update(PlantType.None, DateTime.MaxValue);
                return;
            }

            var timeUntilDone = TimeSpan.FromDays(365);
            await _page.SelectAndDo("#gtt_zeit", async e =>
            {
                // format: "fertig", "hh:mm:ss", "dd:hh:mm:ss"
                var timeString = await (await e.GetPropertyAsync("innerText")).JsonValueAsync() as string;

                if (timeString == null)
                {
                    return;
                }

                if (timeString.Contains("fertig"))
                {
                    timeUntilDone = TimeSpan.Zero;
                    return;
                }

                timeUntilDone = ImperatorHelpers.ParseTimeString(timeString);
            });
            
            tile.Update(type, DateTime.Now + timeUntilDone);

            Console.WriteLine($"Found Tile: {tile.Type}, done at {tile.FinishedAt}");
        });
    }

    #region Helpers

    private Garden _garden = new Garden();
    
    
    
    public delegate Task PerFieldAction(IElementHandle handle, IPage page, Tile tile);
    private async Task PerField(PerFieldAction action)
    {
        if (!IsValid)
        {
            throw new InvalidOperationException("Imperator Session was invalidated.");
        }
        
        foreach (var tile in _garden.Tiles)
        {
            await _page.SelectAndDo(tile.Selector, async e =>
            {
                await action(e, _page!, tile);
            });
        }
    }

    

    #endregion
    
    
}