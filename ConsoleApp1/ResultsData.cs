using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Text;

namespace WasmBenchmarkResults
{
    class JsonResultsData
    {
        public List<BenchTask.Result> results;
        public Dictionary<string, double> minTimes;
        public DateTime timeStamp;

        public static JsonResultsData? Load(string path)
        {
            var options = new JsonSerializerOptions { IncludeFields = true };
            return JsonSerializer.Deserialize<JsonResultsData>(path, options);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            foreach (var result in results)
            {
                stringBuilder.Append(result.ToString() + '\n');
            }

            foreach (var pair in minTimes)
            {
                stringBuilder.Append(pair.Key + " " + pair.Value + "\n");
            }

            stringBuilder.Append(timeStamp);
            return stringBuilder.ToString();
        }
    }

    internal class FlavorData
    {
        public DateTimeOffset commitTime;
        public string runPath;
        public string flavor;
        public JsonResultsData results;

        public HashSet<string> MeasurementLabels => results.minTimes.Keys.ToHashSet<string>();

        public FlavorData(string path, string flavor, JsonResultsData? jsonResultsData, string gitLogContent)
        {
            runPath = path;
            this.flavor = flavor;
            results = jsonResultsData;
            commitTime = LoadGitLog(gitLogContent);
        }

        public DateTimeOffset LoadGitLog(string text)
        {
            var lines = text.Split("\n");
            var regex = new Regex(@"^Date: +(.*)$");
            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (!match.Success)
                    continue;

                var dateString = match.Groups[1].Value;

                if (!DateTimeOffset.TryParseExact(dateString, "ddd MMM d HH:mm:ss yyyy K", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date))
                    continue;

                return date;
            }

            throw new InvalidDataException("unable to load git log data");
        }

        public override string ToString()
        {
            return "\npath: " + runPath + "\nflavor: " + flavor + "\ndata: " + results + "\nCommitTime: " + commitTime;
        }

    }

    internal class ResultsData
    {
        public Dictionary<string, FlavorData> results = new();
    }
}

