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
    public bool IsLoggedIn { get; private set; } = false;

    public async Task LogIn(ImperatorCredentials credentials)
    {
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
            
            await Task.Delay(5000);
            
            if (_page.Url.Contains("main.php"))
            {
                IsLoggedIn = true;
            }
        }
    }
    
}