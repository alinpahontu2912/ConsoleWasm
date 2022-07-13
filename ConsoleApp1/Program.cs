using System.Net;
using System.Text.Json.Nodes;
using System.Text;
using System.Linq;
using System;
using System.Runtime.InteropServices.ComTypes;
using WasmBenchmarkResults;

class Test
{
    private static string getFlavor(string line)
    {
        var words = line.Split("/");
        StringBuilder stringBuilder = new();
        for (var i = 2; i < words.Length - 1; i++)
        {
            stringBuilder.Append(words[i] + ".");
        }

        return stringBuilder.ToString().Remove(stringBuilder.Length - 1);
    }

    static async Task Main(string[] args)
    {
        var client = new HttpClient();
        List<JsonResultsData> jsonResultsData = new();
        List<FlavorData> flavorDatas = new();
        SortedDictionary<DateTimeOffset, ResultsData> timedResults = new();
        client.DefaultRequestHeaders.Add("User-Agent", "my-app");
        var main = "https://raw.githubusercontent.com/radekdoulik/WasmPerformanceMeasurements/main/";
        var response = await client.GetAsync(main + "measurements/jsonDataFiles.txt");
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("HTTP request failed with status code " + response.StatusCode + " and message " + response.ReasonPhrase);
        }
        var text = await response.Content.ReadAsStringAsync();
        var lines = text.Split("\n");
        for (var i = 0; i < lines.Length - 1; i++)
        {
            var fileUrl = lines[i];
            response = await client.GetAsync(main + fileUrl);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("HTTP request failed with status code " + response.StatusCode + " and message " + response.ReasonPhrase);
            }
            var content = response.Content.ReadAsStringAsync().Result;
            var deserializedCont = JsonResultsData.Load(content);
            var logUrl = lines[i].Replace("results.json", "git-log.txt");
            response = await client.GetAsync(main + logUrl);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("HTTP request failed with status code " + response.StatusCode + " and message " + response.ReasonPhrase);
            }
            content = response.Content.ReadAsStringAsync().Result;
            var flavorData = new FlavorData(main + fileUrl, getFlavor(fileUrl), deserializedCont, content);
            ResultsData resultsData;
            if (timedResults.ContainsKey(flavorData.commitTime))
            {
                resultsData = timedResults[flavorData.commitTime];
            }
            else
            {
                resultsData = new ResultsData();
                timedResults[flavorData.commitTime] = resultsData;
            }

            resultsData.results[flavorData.flavor] = flavorData;

        }
        foreach (var item in timedResults)
        {
            Console.WriteLine(item.Key);
            foreach (var elem in item.Value.results.Keys)
            {
                Console.WriteLine(elem + "\n" + item.Value.results[elem]);
            }
        }
    }

}
