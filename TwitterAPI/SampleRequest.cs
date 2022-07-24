using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleTables;
using Newtonsoft.Json.Linq;


namespace TwitterAPI
{
    internal class SampleRequest
    {
        private APIInfo _ApiInfo { get;}
        private Stopwatch _stopwatch { get; }

        private readonly List<string> Tweets;

        private AuthenticationHeaderValue _AuthHeader => new ("Bearer", _ApiInfo.Token);
        

        public SampleRequest(APIInfo apiInfo)
        {
            Tweets = new List<string>();
            _stopwatch = Stopwatch.StartNew();
            _ApiInfo = apiInfo;
        }

        public async Task GetTwitterSampleStream()
        {
            using HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromHours(6);

            using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, $"{_ApiInfo.TweetsUrl}/tweets/sample/stream");
            req.Headers.Authorization = _AuthHeader;

            using HttpResponseMessage response = await httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            await WriteStreamResult(stream);
        }

        private async Task WriteStreamResult(Stream stream)
        {
            using var streamReader = new StreamReader(stream, Encoding.UTF8);
            int lastQuotient = 0;

            while (!streamReader.EndOfStream)
            {
                //Report results after a defined time interval
                //Limited to a 24 hour run
                int seconds = _stopwatch.Elapsed.Seconds;
                int minuteSecs = _stopwatch.Elapsed.Minutes * 60;
                int hourSecs = _stopwatch.Elapsed.Hours * 3600;
                int totalSecs = seconds + minuteSecs + hourSecs;

                int currentQuotient = totalSecs / _ApiInfo.MeasureIntervalSeconds;

                if (totalSecs > 0 && currentQuotient > lastQuotient)
                {
                    lastQuotient = totalSecs / _ApiInfo.MeasureIntervalSeconds;
                    string tweetContents = string.Join(" ", Tweets);
                    IEnumerable<KeyValuePair<string, int>> mostCommon = GetMostFrequentHashTags(tweetContents);
                    var table = new ConsoleTable("Hashtag", "Frequency");

                    foreach (KeyValuePair<string, int> tag in mostCommon)
                    {
                        table.AddRow(tag.Key, tag.Value);
                    }

                    Console.Clear();
                    Console.WriteLine($"Total tweets over {_stopwatch.Elapsed.Hours}:{_stopwatch.Elapsed.Minutes}:{_stopwatch.Elapsed.Seconds} seconds: {Tweets.Count}.");
                    Console.WriteLine();
                    Console.WriteLine($"Top {_ApiInfo.MostFrequentHashTags} hashtags:");

                    table.Write(Format.Alternative);
                }

                string response = await streamReader.ReadLineAsync();
                try
                {
                    string tweet = JObject.Parse(response)["data"]["text"].Value<string>();
                    Tweets.Add(tweet);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private IEnumerable<KeyValuePair<string, int>> GetMostFrequentHashTags(string contents)
        {
            var pattern = new Regex("#[^\\s!@#$%^&*()=+.\\/,\\[{\\]};:'\"?><]+");
            MatchCollection matches = pattern.Matches(contents);
            return matches
                .GroupBy(m => m.Value)
                .Select(x => new KeyValuePair<string, int>(x.Key, x.Count()))
                .OrderByDescending(x => x.Value)
                .Take(_ApiInfo.MostFrequentHashTags);
        }
    }

}
