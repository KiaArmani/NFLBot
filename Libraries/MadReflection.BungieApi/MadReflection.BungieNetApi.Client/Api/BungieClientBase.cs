using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BungieNet.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BungieNet.Api
{
    public abstract class BungieClientBase
    {
        private readonly string _apiKey;
        private string _bearerToken;


        private protected BungieClientBase(IBungieApiKey apiKey)
        {
            if (apiKey == null)
                throw new ArgumentNullException(nameof(apiKey));
            if (apiKey.Value == "")
                throw new ArgumentException("API key value cannot be an empty string.",
                    nameof(apiKey) + "." + nameof(apiKey.Value));

            _apiKey = apiKey.Value;
        }


        public void SetBearerToken(string bearerToken)
        {
            if (bearerToken == null)
                throw new ArgumentNullException(nameof(bearerToken));
            if (string.IsNullOrEmpty(bearerToken))
                throw new ArgumentException("Bearer token cannot be an empty string.", nameof(bearerToken));

            _bearerToken = bearerToken;
        }

        public void ClearBearerToken()
        {
            _bearerToken = null;
        }

        private HttpClient GetHttpClient()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Accept", "text/json");
            if (_bearerToken != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
            client.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
            return client;
        }

        private async Task<string> GetResourceAsync(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));


            using (var client = GetHttpClient())
            {
                var response = await client.GetAsync(uri.ToString());

                return await response.Content.ReadAsStringAsync();
            }
        }

        internal Uri GetEndpointUri(string[] pathSegments, bool includeTrailingSlash,
            IEnumerable<QueryStringItem> queryStringItems = null, bool d1 = false, bool stats = false)
        {
            if (pathSegments == null)
                throw new ArgumentNullException(nameof(pathSegments));
            if (pathSegments.Length == 0)
                throw new ArgumentException("No endpoint path.");

            foreach (var pathSegment in pathSegments)
            {
                if (pathSegment == null)
                    throw new ArgumentNullException(nameof(pathSegment), "Array elements cannot be null.");
                if (pathSegments.Length == 0)
                    throw new ArgumentException("Array element is empty.", nameof(pathSegments));
            }

            var url = d1 ? Constants.BaseUriD1 : Constants.BaseUri;
            if (url == Constants.BaseUri && stats)
                url = Constants.BaseUriStats;

            var builder = new UriBuilder(url);

            builder.Path += string.Join("/", pathSegments);
            if (includeTrailingSlash)
                builder.Path += "/";

            if (queryStringItems != null && queryStringItems.Any())
            {
                var queryString = new StringBuilder();

                foreach (var queryStringItem in queryStringItems)
                    queryString.Append(WebUtility.UrlEncode(queryStringItem.Name)).Append('=')
                        .Append(WebUtility.UrlEncode(queryStringItem.Value)).Append("&");

                if (queryString.Length > 0 && queryString[queryString.Length - 1] == '&')
                    queryString.Length--;

                builder.Query = queryString.ToString();
            }

            return builder.Uri;
        }

        private Task<string> PostResourceAsync(Uri uri)
        {
            return PostResourceAsync(uri, "");
        }

        private async Task<string> PostResourceAsync(Uri uri, string postBody)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var request = new StringContent(postBody);

            using (var client = GetHttpClient())
            {
                var response = await client.PostAsync(uri.ToString(), request);

                return await response.Content.ReadAsStringAsync();
            }
        }

        private async Task<JToken> GetObjectAsync(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var response = await GetResourceAsync(uri);

            try
            {
                var jObject = JObject.Parse(response);
                var errorCode = (int) jObject["ErrorCode"];
                if (errorCode != 1)
                    throw new BungieException((PlatformErrorCodes) errorCode, (string) jObject["ErrorStatus"],
                        (string) jObject["Message"], jObject["MessageData"]);

                return jObject["Response"];
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task<JToken> PostObjectAsync(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var response = await PostResourceAsync(uri);

            var jObject = JObject.Parse(response);

            var errorCode = (int) jObject["ErrorCode"];
            if (errorCode != 1)
                throw new BungieException((PlatformErrorCodes) errorCode, (string) jObject["ErrorStatus"],
                    (string) jObject["Message"], jObject["MessageData"]);

            return jObject["Response"];
        }

        private async Task<JToken> PostObjectAsync(Uri uri, JToken inputObject)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var request = inputObject.ToString(Formatting.None);

            var response = await PostResourceAsync(uri, request);

            var jObject = JObject.Parse(response);

            var errorCode = (int) jObject["ErrorCode"];
            if (errorCode != 1)
                throw new BungieException((PlatformErrorCodes) errorCode, (string) jObject["ErrorStatus"],
                    (string) jObject["Message"], jObject["MessageData"]);

            return jObject["Response"];
        }

        protected async Task<TOutput> GetEntityAsync<TOutput>(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var response = (JObject) await GetObjectAsync(uri);
            if (response is null)
                return default;

            return response.ToObject<TOutput>();
        }

        protected async Task<TOutput> PostEntityAsync<TOutput>(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var response = (JObject) await PostObjectAsync(uri);

            return response.ToObject<TOutput>();
        }

        protected async Task<TOutput> PostEntityAsync<TInput, TOutput>(Uri uri, TInput inputObject)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            JToken request = JsonConvert.SerializeObject(inputObject, Formatting.None);

            var response = (JObject) await PostObjectAsync(uri, request);

            return response.ToObject<TOutput>();
        }

        protected async Task<TOutput[]> GetEntityArrayAsync<TOutput>(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var response = (JArray) await GetObjectAsync(uri);
            if (response is null)
                return default;

            return response.ToObject<TOutput[]>();
        }

        protected async Task<TOutput[]> PostEntityArrayAsync<TOutput>(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var response = (JArray) await PostObjectAsync(uri);

            return response.ToObject<TOutput[]>();
        }

        protected async Task<TOutput[]> PostEntityArrayAsync<TInput, TOutput>(Uri uri, TInput inputObject)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            JToken request = JsonConvert.SerializeObject(inputObject, Formatting.None);

            var response = (JArray) await PostObjectAsync(uri, request);

            return response.ToObject<TOutput[]>();
        }
    }
}