using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XurClassLibrary.Models;

namespace XurBot.Modules
{
    public static class ApiConnectorModule
    {
        private static readonly HttpClient Client;
        private static readonly string BaseUrl = Environment.GetEnvironmentVariable("XUR_MONGOBRIDGE_URL");

        static ApiConnectorModule()
        {
            Client = new HttpClient();
        }

        public static async Task<int> GetPositionOfScoreAsync(long nightfallId)
        {
            var apiUrlBuilder = new StringBuilder();
            apiUrlBuilder.Append(BaseUrl);
            apiUrlBuilder.Append("/api/nfl/scores/position/");
            apiUrlBuilder.Append(nightfallId);

            var httpResult = await GetHttp(apiUrlBuilder.ToString());
            return Convert.ToInt32(httpResult);
        }

        public static async Task<List<ScoreEntry>> GetTopOrdealScoresAsync(int topX)
        {
            var apiUrlBuilder = new StringBuilder();
            apiUrlBuilder.Append(BaseUrl);
            apiUrlBuilder.Append("/api/nfl/scores/top/");
            apiUrlBuilder.Append(topX);

            var httpResult = await GetHttp(apiUrlBuilder.ToString());
            return JsonConvert.DeserializeObject<List<ScoreEntry>>(httpResult);
        }

        private static async Task<string> GetHttp(string url)
        {
            var response = await Client.GetAsync(url);
            if (response.IsSuccessStatusCode) return await response.Content.ReadAsStringAsync();

            return null;
        }
    }
}