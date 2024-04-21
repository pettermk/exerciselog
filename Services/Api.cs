using System.Net.Http.Headers;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Net.Http;

namespace exerciselog.Services;

class Api {
    private readonly IJSRuntime _jsRuntime;
    public Api(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    class UserData
    {
        public string id_token { get; set; }
        public int expires_at { get; set; }
    }

    private async Task<string> GetToken() {
        var userDataKey = "oidc.user:https://homeautomation-api.kvalvaag-tech.com/o:I9xkPt8LYNR81aOK8Xz1z4ppCiYz9CvEdF6fG3ud";
        var userData = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", userDataKey);
        var user = JsonSerializer.Deserialize<UserData>(userData);
        return user.id_token;
    }

    private async Task<AuthenticationHeaderValue> GetAuthorizationHeader() {
        return new AuthenticationHeaderValue("Bearer", await GetToken());
    }

    public async Task PostExercise(string Name, string Description)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://exerciselog-api.kvalvaag-tech.com/api/exercise");
        var postData = new {
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
}