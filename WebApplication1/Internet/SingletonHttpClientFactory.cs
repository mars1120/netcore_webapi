namespace WebApplication1.Internet;

public class SingletonHttpClientFactory : IHttpClientFactory
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public HttpClient CreateClient(string name)
    {
        return _httpClient;
    }
}