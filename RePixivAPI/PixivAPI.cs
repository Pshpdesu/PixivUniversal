using RePixivAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static RePixivAPI.Helpers.AuthenticationConstants;

namespace RePixivAPI
{
    public class Tokens
    {
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
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
        {}


        private async Task Authorize(GrantType grantType, Credentials credentials)
        {
            userCredentials = credentials;
            var client = GetDefaultAuthHttpClient();
            var urlData = new FormUrlEncodedContent(GetAuthorizeContent(grantType, this.userCredentials));

            var response = await client.PostAsync(ApiUrls.AuthUrl, urlData);
            var text = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
        }
        private HttpClient GetDefaultAuthHttpClient()
        {
            var httpClient = new HttpClient();
            var headers = GetAuthHeaders();
            foreach(var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            return httpClient;
        }

        //public class AuthResult
        //{
        //    public Tokens Tokens;
        //    public Authorize Authorize;
        //    public AuthKey Key;
        //}
    }
}
