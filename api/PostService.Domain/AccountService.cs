namespace PostService.Domain;
public interface IAccountService
{
    Task<string> GetAuthorNameAsync(Guid authorId);
}

public class AccountService : IAccountService
{
    private readonly HttpClient _httpClient;

    public AccountService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetAuthorNameAsync(Guid authorId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://localhost:7234/api/accounts/{authorId}/name");
            response.EnsureSuccessStatusCode();

            var username = await response.Content.ReadAsStringAsync();
            return username;
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }
}
