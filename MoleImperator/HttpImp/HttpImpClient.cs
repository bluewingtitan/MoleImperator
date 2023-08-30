namespace MoleImperator.HttpImp;

public class HttpImpClient
{
    private readonly HttpClient _client;
    private ImpSession _session;

    public HttpImpClient()
    {
        _session = new ImpSession();
        _client = ConstructClient();
    }

    private static HttpClient ConstructClient()
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback = 
            (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            };

        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36 Vivaldi/2.2.1388.37");

        return client;
    }
    
}