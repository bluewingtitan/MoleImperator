using System.Security.Authentication;
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
    /// Molehill Empire invalidates sessions after 2h, ImperatorSession gets invalidated after 1h 45m to keep it simple.
    /// </summary>
    public bool IsValid => ActiveTime.TotalMinutes < (60 + 45) && IsLoggedIn;
    
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
                await ScanJars();
                await ScanFields(false);
            }
        }
    }


    #region InGarden

    private async Task EnsureIsInGarden()
    {
        if (!IsValid)
        {
            throw new InvalidCredentialException("ImperatorSession is invalid. Ensure it's logged in with correct credentials and the underlying MolehillEmpire-Session is valid too.");
        }
        
        
    }
    
    public async Task ScanJars()
    {
        _garden.SeedAmounts.Clear();
        foreach (var (type, data) in PlantTypeData.Data)
        {
            await _page.SelectAndDo(data.SeedAmountSelector, async e =>
            {
                var amount = await (await e.GetPropertyAsync("innerText")).JsonValueAsync() as string;
                if (int.TryParse(amount, out var amountNum))
                {
                    _garden.SeedAmounts[type] = amountNum;
                }
            });
            
        }
    }
     
    public async Task ScanFields(bool skipWeeds = true)
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
            
            // weed?
            if ((uint) type > 1000)
            {
                // Is weed.
                tile.Update(type, DateTime.MaxValue);
                return;
            }

            var timeUntilDone = TimeSpan.FromDays(365);
            await _page!.SelectAndDo("#gtt_zeit", async e =>
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
        }, skipWeeds);
    }

    public int GetFreeTileCount()
    {
        return _garden.Tiles.Count(tile => tile.Type == PlantType.None);
    }

    public async Task<float> GetMoney()
    {
        var money = -1f;
        await _page!.SelectAndDo("#bar", async e =>
        {
            var text = await (await e.GetPropertyAsync("innerText")).JsonValueAsync() as string;
            if (text != null)
            {
                float.TryParse(text, out money);
            }
        });

        return money;
    }

    public async Task RemoveWeedsFor(float cap)
    {
        var order = new Stack<PlantType>();
        order.Push(PlantType.Weeds_XL);
        order.Push(PlantType.Weeds_L);
        order.Push(PlantType.Weeds_M);
        order.Push(PlantType.Weeds_S);

        var totalCost = 0f;
        
        while (totalCost < cap && order.Count > 0)
        {
            var type = order.Pop();
            var data = PlantTypeData.Data[type];
            if (totalCost + data.RefPrice > cap)
            {
                // skip. to expensive.
                continue;
            }
            
            foreach (var tile in _garden.Tiles)
            {
                if (tile.Type == type)
                {
                    if (totalCost + data.RefPrice > cap)
                    {
                        // skip. to expensive.
                        continue;
                    }
                    
                    // Remove.
                    await _page!.SelectAndDo(tile.Selector, e => e.ClickAsync());
                    await Task.Delay(100);
                    await _page!.SelectAndDo("#baseDialogButton", e => e.ClickAsync());
                    await Task.Delay(250);
                    await _page!.RemovePopups();
                    
                    tile.Update(PlantType.None, DateTime.MaxValue);

                    totalCost += data.RefPrice;
                }
            }
        }

        await ScanFields();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    /// <returns>Amount of actually planted seeds.</returns>
    public async Task<int> Plant(PlantType type, int amount)
    {
        // click seed jar
        if (!PlantTypeData.Data.TryGetValue(type, out var typeData))
        {
            throw new ArgumentException($"Plant Type {type} is not plantable.");
        }
        
        // enough seeds?
        await ScanJars();
        if (!_garden.SeedAmounts.TryGetValue(type, out var availableSeeds))
        {
            // no seeds at all.
            return 0;
        }

        var totalPlantsPossible = Math.Min(amount, availableSeeds);

        var jarClicked = false;
        await _page!.SelectAndDo(typeData.JarSelector, async element =>
        {
            jarClicked = true;
            await element.ClickAsync();
        });

        if (!jarClicked)
        {
            return 0;
        }

        var planted = 0;
        await PerField(async (handle, page, tile) =>
        {
            if (planted >= totalPlantsPossible)
            {
                return;
            }
            
            if (tile.Type == PlantType.None)
            {
                planted++;
                await handle.ClickAsync();
                await Task.Delay(200);
                tile.Update(type, DateTime.Now + TimeSpan.FromSeconds(typeData.GrowTimeSeconds));
            }
        });

        return planted;
    }

    public async Task HarvestAll()
    {
        await _page!.EvaluateExpressionAsync("gardenjs.harvestAll()");
        await Task.Delay(2500);
        await _page.CloseHarvestPopUps();
        
        // We need a rescan now. (Still faster than harvesting bit by bit as it's not sending and waiting for requests and clicking through pop-ups this way)
        await ScanFields();
        await ScanJars();
    }

    #region Helpers

    private Garden _garden = new Garden();
    
    public delegate Task PerFieldAction(IElementHandle handle, IPage page, Tile tile);
    private async Task PerField(PerFieldAction action, bool skipWeeds = true)
    {
        if (!IsValid)
        {
            throw new InvalidOperationException("Imperator Session was invalidated.");
        }
        
        foreach (var tile in _garden.Tiles)
        {
            if (skipWeeds && (uint)tile.Type > 1000)
            {
                continue;
            }
            
            await _page!.SelectAndDo(tile.Selector, async e =>
            {
                await action(e, _page!, tile);
            });
        }
    }

    

    #endregion

    #endregion


    #region Navigate

    public enum ImperatorPage
    {
        Garden1,
        Village,
        Market,
        
    }

    private ImperatorPage currentPage = ImperatorPage.Garden1;

    public async Task NavigateToGarden1()
    {
        switch (currentPage)
        {
            case ImperatorPage.Garden1:
                return;
            
            case ImperatorPage.Market:
                await _page.EvaluateExpressionAsync("parent.parent.stadt_schliesseFrame();");
                await Task.Delay(2000);
                await _page.EvaluateExpressionAsync("parent.stadtVerlassen();");
                break;
            
            case ImperatorPage.Village:
                await _page.EvaluateExpressionAsync("parent.stadtVerlassen();");
                break;
                
                
        }
        await Task.Delay(2000);
    }
    
    public async Task NavigateTo(ImperatorPage page)
    {
        if (page == currentPage)
        {
            return;
        }

        // navigate home, then navigate from home TO the wanted page.
        // not always the most efficient way of doing things, but the easiest to manage in the long run.
        // if you want a small project:
        // this would be a great use-case of a graph, needed transition-calls linked to the directional connections and pathfinding between the nodes (pages).
        await NavigateToGarden1();

        currentPage = page;
        
        switch (page)
        {
            case ImperatorPage.Village:
                await _page.EvaluateExpressionAsync("wimparea.openMap()");
                break;
            
            case ImperatorPage.Garden1:
                return;
            
            case ImperatorPage.Market:
                await _page.EvaluateExpressionAsync("wimparea.openMap()");
                await Task.Delay(2000);
                await _page.EvaluateExpressionAsync("parent.zeige('markt')");
                break;
        }
        
        await Task.Delay(2000);
    }
    

    #endregion
    
}