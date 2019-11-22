using RePixivAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static RePixivAPI.Helpers.AuthenticationConstants;

namespace RePixivAPI
{
    public class Tokens
    {
        [JsonProperty, JsonRequired]
        public string RefreshToken { get; set; }
        [JsonProperty, JsonRequired]
        public string AccessToken { get; set; }
        [JsonProperty(PropertyName = "ExpiresIn")]
        private int _ExpiresIn;
        [JsonIgnore]
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
    }

    public class Credentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public Tokens Tokens { get; set; }
    }

    public class PixivAPIClient
    {
        public static async Task<PixivAPIClient> GetPixivApiClient(GrantType grantType, Credentials credentials)
        {
            PixivAPIClient client = new PixivAPIClient();
            await client.Authorize(grantType, credentials);
            return client;
        }


        Credentials userCredentials { get; set; }

        private PixivAPIClient()
        { }

        private async Task Authorize(GrantType grantType, Credentials credentials)
        {
            this.userCredentials = credentials;
            await Authorize(grantType);
        }

        private async Task Authorize(GrantType grantType)
        {
            //userCredentials = credentials;
            var client = GetDefaultAuthHttpClient();
            var urlData = new FormUrlEncodedContent(GetAuthorizeContent(grantType, this.userCredentials));
            var test = await urlData.ReadAsStringAsync();
            var response = await client.PostAsync(ApiUrls.AuthUrl, urlData);
            var text = await response.Content.ReadAsStringAsync();
            var text2 = response.RequestMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
        private HttpClient GetDefaultAuthHttpClient()
        {
            var httpClient = new HttpClient();
            var headers = GetAuthHeaders();
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            return httpClient;
        }
        private  async Task CheckAuthorizationAsync()
        {
            if (userCredentials.Tokens.ExpirationTime >= DateTime.UtcNow)
            {
                await Authorize(GrantType.RefreshToken);
            }
        }

        //public class AuthResult
        //{
        //    public Tokens Tokens;
        //    public Authorize Authorize;
        //    public AuthKey Key;
        //}
    }
}
