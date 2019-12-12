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
            ExpiresIn = authres.ExpiresIn??0;
        }
    }

    public class Credentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public Tokens Tokens { get; set; }
    }

    public class PixivAPICore: ISuspendable
    {
        Credentials userCredentials { get; set; }
        HttpClient authClient { get; set; }
        HttpClient apiClient { get; set; }
        HttpClient authApiClient { get; set; }

        protected PixivAPICore()
        {
            authClient = GetDefaultAuthClient();
            apiClient = GetDefaultApiClient();
        }

        protected async Task Authorize(GrantType grantType, Credentials credentials)
        {
            this.userCredentials = credentials;
            var tokens = await Authorize(grantType);

            authApiClient = GetDefaultAuthApiHttpClient(tokens);
        }

        protected async Task<Tokens> Authorize(GrantType grantType)
        {
            //userCredentials = credentials;
            var urlData = new FormUrlEncodedContent(GetAuthorizeContent(grantType, this.userCredentials));
            var response = await authClient.PostAsync(RePixivAPI.Helpers.ApiUrls.AuthUrl, urlData);
            response.EnsureSuccessStatusCode();
            var result = new Tokens(JsonConvert.DeserializeObject<Response<Authorize>>(await response.Content.ReadAsStringAsync()).response);
            userCredentials.Tokens = result;
            return result;

        }

        internal HttpClient GetDefaultAuthClient()
        {
            var httpClient = new HttpClient();
            var headers = GetAuthHeaders();
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            return httpClient;
        }

        internal HttpClient GetDefaultApiClient()
        {
            var httpClient = new HttpClient();
            return httpClient;
        }

        internal HttpClient GetDefaultAuthApiHttpClient(Tokens tokens)
        {
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokens.AccessToken);


            return httpClient;
        }

        protected  async Task CheckAuthorizationAsync()
        {
            if (userCredentials.Tokens.ExpirationTime >= DateTime.UtcNow)
            {
                var tokens = await Authorize(GrantType.RefreshToken);

                //authApiClient.Dispose();
            }
        }

        protected async Task<HttpResponseMessage> SendAuthorizedApiRequestAsync(HttpMethod method, Uri uri, IDictionary<string,string> @params, System.Threading.CancellationToken cancellationToken= default)
        {
            await CheckAuthorizationAsync();
            HttpRequestMessage reqst = new HttpRequestMessage(method, uri);
            reqst.Content = new FormUrlEncodedContent(@params);
            return await authApiClient.SendAsync(reqst,cancellationToken: cancellationToken);
        }

        protected async Task<HttpResponseMessage> SendApiRequestAsync(HttpMethod method, Uri uri, IDictionary<string, string> @params, System.Threading.CancellationToken cancellationToken = default)
        {
            HttpRequestMessage reqst = new HttpRequestMessage(method, uri);
            reqst.Content = new FormUrlEncodedContent(@params);
            return await apiClient.SendAsync(reqst, cancellationToken: cancellationToken);
        }

        public void SuspendHttpClients()
        {
            authApiClient.Dispose();
            apiClient.Dispose();
            authClient.Dispose();
        }

        public void ResumeHttpClients()
        {
            authClient = GetDefaultAuthClient();
            apiClient = GetDefaultApiClient();
            authApiClient = GetDefaultAuthApiHttpClient(userCredentials.Tokens);
        }
        //public class AuthResult
        //{
        //    public Tokens Tokens;
        //    public Authorize Authorize;
        //    public AuthKey Key;
        //}
    }
}
