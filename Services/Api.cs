using System.Net.Http.Headers;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace exerciselog.Services;

class Api
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IConfiguration _config;
    public Api(IJSRuntime jsRuntime, IConfiguration config)
    {
        _jsRuntime = jsRuntime;
        _config = config;
    }

    class UserData
    {
        public string id_token { get; set; }
        public int expires_at { get; set; }
    }


    public string GetUserDataKey()
    {
        var clientId = _config["Local:ClientId"];
        Console.WriteLine(clientId);
        return $"oidc.user:https://homeautomation-api.kvalvaag-tech.com/o:{clientId}";
    }

    private async Task<string> GetToken()
    {
        var userDataKey = GetUserDataKey();
        var userData = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", userDataKey);
        var user = JsonSerializer.Deserialize<UserData>(userData);
        return user.id_token;
    }



    private async Task<AuthenticationHeaderValue> GetAuthorizationHeader()
    {
        return new AuthenticationHeaderValue("Bearer", await GetToken());
    }



    public async Task PostExercise(string Name, string Description)

    {

        var request = new HttpRequestMessage(HttpMethod.Post, "https://exerciselog-api.kvalvaag-tech.com/api/exercise");
        var postData = new
        {
            name = Name,
            description = Description
        };

        var json = JsonSerializer.Serialize(postData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        request.Content = content;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());

        var httpClient = new HttpClient();
        var response = await httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("POST request successful. Response: " + responseContent);
        }
    }


    public async Task PostTimeseries(TimeseriesDto Ts)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://exerciselog-api.kvalvaag-tech.com/api/timeseries");

        var json = JsonSerializer.Serialize(Ts);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        request.Content = content;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());

        var httpClient = new HttpClient();
        var response = await httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("POST request successful. Response: " + responseContent);
        }
    }

    public async Task<List<string>> GetUniqueTimeseriesDimensions()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://exerciselog-api.kvalvaag-tech.com/api/DistinctDimensions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetToken());

        var httpClient = new HttpClient();
        var response = await httpClient.SendAsync(request);
        var UniqueDimensions = await response.Content.ReadFromJsonAsync<List<string>>();

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("POST request successful. Response: " + responseContent);
        }

        return UniqueDimensions;
    }
}

public class TimeseriesDto
{
    public int Id { get; set; }
    public double Value { get; set; }
    public string Timestamp { get; set; } = "";
    public string Dimension { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public string? Metadata { get; set; }
}
