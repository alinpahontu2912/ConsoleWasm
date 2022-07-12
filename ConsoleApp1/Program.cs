using System.Net;
using System.Text.Json.Nodes;
using System.Text;
using System.Linq;
using System;

class Test
{
    static async Task Main(string[] args)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "my-app");
        string main = "https://raw.githubusercontent.com/radekdoulik/WasmPerformanceMeasurements/main/";
        HttpResponseMessage response = await client.GetAsync(main + "measurements/jsonDataFiles.txt");
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("HTTP request failed with status code " + response.StatusCode + "and message " + response.ReasonPhrase);
        }
        var text = await response.Content.ReadAsStringAsync();
        var lines = text.Split("\n");
        for (int i = 0; i < lines.Length; i++)
        {
            string? fileUrl = lines[i];
            response = await client.GetAsync(main + fileUrl);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("HTTP request failed with status code " + response.StatusCode + "and message " + response.ReasonPhrase);
            }
            JsonObject obj = JsonNode.Parse(response.Content.ReadAsStringAsync().Result).AsObject();
            Console.WriteLine(obj);
        }

    }

}
