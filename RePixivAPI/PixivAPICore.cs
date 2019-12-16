using RePixivAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static RePixivAPI.Helpers.AuthenticationConstants;
using RePixivAPI.Objects;
using System.Threading;
using System.Net.Http.Headers;
using System.Net;

namespace RePixivAPI
{
    public class Tokens
    {
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        private int _ExpiresIn;
        public int ExpiresIn
        {
            get
            {
                return _ExpiresIn;
            }
            set
            {
                ExpirationTime = DateTime.UtcNow.AddSeconds(value);
                _ExpiresIn = value;
            }
        }
        public DateTime ExpirationTime { get; set; }
        public Tokens() { }
        public Tokens(Authorize authres)
        {
            RefreshToken = authres.RefreshToken;
            AccessToken = authres.AccessToken;
            ExpiresIn = authres.ExpiresIn ?? 0;
        }
    }

    public class Credentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public Tokens Tokens { get; set; }
    }

    public class PixivAPICore : ISuspendable
    {
        private static Mutex authMutex { get; set; } = new Mutex();
        Credentials userCredentials { get; set; }
        HttpClient apiClient { get; set; }
        HttpClient authApiClient { get; set; }
        HttpClientHandler defaultHandler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        protected PixivAPICore()
        {
            apiClient = GetDefaultPublicApiClient();
        }

        protected async Task Authorize(GrantType grantType, Credentials credentials)
        {
            this.userCredentials = credentials;
            var tokens = await Authorize(grantType);

            authApiClient = GetDefaultAuthorizedApiHttpClient(tokens);
        }

        protected async Task<Tokens> Authorize(GrantType grantType)
        {
            //userCredentials = credentials;
            var urlData = new FormUrlEncodedContent(GetAuthorizeContent(grantType, this.userCredentials));
            using (var authClient = GetDefaultAuthClient())
            using (var response = await authClient.PostAsync(RePixivAPI.Helpers.ApiUrls.AuthUrl, urlData))
            {
                response.EnsureSuccessStatusCode();
                var result = new Tokens(JsonConvert.DeserializeObject<Response<Authorize>>(await response.Content.ReadAsStringAsync()).response);
                userCredentials.Tokens = result;
                return result;
            }
        }

        internal HttpClient GetDefaultAuthClient()
        {
            var httpClient = new HttpClient(defaultHandler);
            var headers = GetAuthHeaders();
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            return httpClient;
        }

        internal HttpClient GetDefaultPublicApiClient()
        {
            var httpClient = new HttpClient(defaultHandler);
            return httpClient;
        }

        internal HttpClient GetDefaultAuthorizedApiHttpClient(Tokens tokens)
        {
            var httpClient = new HttpClient(defaultHandler);
            return httpClient;
        }

        protected async Task CheckAuthorizationAsync()
        {
            authMutex.WaitOne();
            if (userCredentials.Tokens.ExpirationTime >= DateTime.UtcNow)
            {
                await Authorize(GrantType.RefreshToken);
            }
            authMutex.ReleaseMutex();
        }

        protected async Task<T> SendAuthorizedApiRequestAsync<T>(
            HttpMethod method,
            Uri uri,
            IDictionary<string, string> @params,
            System.Threading.CancellationToken cancellationToken = default)
        {
            using (var resp = await SendAuthorizedApiRequestAsync(method, uri, @params, cancellationToken))
            {
                return await ProjectAsync<T>(await resp.Content.ReadAsStringAsync());
            }
        }
        protected async Task<HttpResponseMessage> SendAuthorizedApiRequestAsync(
            HttpMethod method,
            Uri uri,
            IDictionary<string, string> @params,
            System.Threading.CancellationToken cancellationToken = default)
        {
            await CheckAuthorizationAsync();
            return await SendRequest(authApiClient, method, uri, @params, cancellationToken);
        }

        protected async Task<T> SendPublicApiRequestAsync<T>(
            HttpMethod method,
            Uri uri,
            IDictionary<string, string> @params,
            System.Threading.CancellationToken cancellationToken = default)
        {
            using (var resp = await SendPublicApiRequestAsync(method, uri, @params, cancellationToken))
            {
                return await ProjectAsync<T>(await resp.Content.ReadAsStringAsync());
            }
        }

        protected async Task<HttpResponseMessage> SendPublicApiRequestAsync(
            HttpMethod method,
            Uri uri,
            IDictionary<string, string> @params,
            System.Threading.CancellationToken cancellationToken = default)
        {
            return await SendRequest(apiClient, method, uri, @params, cancellationToken);
        }

        //TODO: Figure it out if that header is neccessary
        //In PixeezAPI they add this header to defaultheaders
        //httpClient.DefaultRequestHeaders.Add("Referer", "https://app-api.pixiv.net/");
        private async Task<HttpResponseMessage> SendRequest(
            HttpClient client,
            HttpMethod method,
            Uri uri,
            IDictionary<string, string> @params,
            CancellationToken cancellationToken)
        {

            using (HttpRequestMessage reqst = new HttpRequestMessage(method, uri)
            { Content = new FormUrlEncodedContent(@params) })
            {
                reqst.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userCredentials.Tokens.AccessToken);
                using (var resp = await SendRequest(client, reqst, cancellationToken))
                {
                    return resp.EnsureSuccessStatusCode();
                }
            }
        }

        private async Task<HttpResponseMessage> SendRequest(HttpClient client, HttpRequestMessage reqst, CancellationToken cancellationToken)
        {
            return await client.SendAsync(reqst, cancellationToken);
        }

        public void Suspend()
        {
            authApiClient.Dispose();
            apiClient.Dispose();
        }

        public void Resume()
        {
            apiClient = GetDefaultPublicApiClient();
            authApiClient = GetDefaultAuthorizedApiHttpClient(userCredentials.Tokens);
        }

        protected async Task<T> ProjectAsync<T>(HttpResponseMessage data)
        {
            return await ProjectAsync<T>(await data.Content.ReadAsStringAsync());
        }

        protected async Task<T> ProjectAsync<T>(string data)
        {
            return await Task.Run(() => JsonConvert.DeserializeObject<T>(data));
        }

        protected async Task<T> ProjectResponseAsync<T>(HttpResponseMessage data)
        {
            return await ProjectResponseAsync<T>(await data.Content.ReadAsStringAsync());
        }

        protected async Task<T> ProjectResponseAsync<T>(string data)
        {
            return await Task.Run(() => JsonConvert.DeserializeObject<Response<T>>(data).response);
        }
        //public class AuthResult
        //{
        //    public Tokens Tokens;
        //    public Authorize Authorize;
        //    public AuthKey Key;
        //}
    }
}
