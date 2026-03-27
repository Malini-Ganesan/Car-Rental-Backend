using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class NodeRedService
{
    private readonly HttpClient _httpClient;

    public NodeRedService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendEvent(string eventType, string description, object data = null)
    {
        var payload = new
        {
            eventType = eventType,
            description = description,
            data = data
        };

        var json = JsonSerializer.Serialize(payload);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("http://localhost:1882/log-event", content);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Node-RED failed");
        }
    }
}