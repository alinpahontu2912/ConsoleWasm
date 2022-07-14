using System.Net;
using System.Text;
using WasmBenchmarkResults;

class Program
{
    readonly static string main = "https://raw.githubusercontent.com/radekdoulik/WasmPerformanceMeasurements/main/";
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
        QuerySolver querySolver = new();
        SortedDictionary<DateTimeOffset, ResultsData> timedResults = new();
        var text = querySolver.solveQuery(main + "measurements/jsonDataFiles.txt").Result;
        var lines = text.Split("\n");
        for (var i = 0; i < lines.Length - 1; i++)
        {
            var fileUrl = lines[i];
            var json = querySolver.solveQuery(main + fileUrl).Result;
            var logUrl = lines[i].Replace("results.json", "git-log.txt");
            var content = querySolver.solveQuery(main + logUrl).Result;
            var flavorData = new FlavorData(main + fileUrl, getFlavor(fileUrl), json, content);
            ResultsData resultsData;
            if (timedResults.ContainsKey(flavorData.commitTime))
                resultsData = timedResults[flavorData.commitTime];
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
    internal class QuerySolver
    {
        public HttpClient client;
        public QuerySolver()
        {
            client = new();
            client.DefaultRequestHeaders.Add("User-Agent", "my-app");
        }

        public async Task<string> solveQuery(string url)
        {
            var response = await client.GetAsync(url);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("HTTP request failed with status code " + response.StatusCode + " and message " + response.ReasonPhrase);
            var text = await response.Content.ReadAsStringAsync();
            return text;
        }
    }



}
