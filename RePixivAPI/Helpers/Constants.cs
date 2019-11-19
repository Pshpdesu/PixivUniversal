using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RePixivAPI.Helpers
{
    public static class AuthenticationConstants
    {
        private const string HashSecret = "28c1fdd170a5204386cb1313c7077b34f83e4aaf4aa829ce78c231e05b0bae2c";

        public enum GrantType
        {
            Password,
            RefreshToken
        }

        static private Dictionary<string, string> oldAuthConsts { get; set; } =
            new Dictionary<string, string>()
            {
                {"App-OS", "ios"},
                {"App-OS-Version", "10.2.1"},
                {"App-Version", "7.1.0"},
                {"User-Agent", "PixivIOSApp/7.1.0 (iOS 10.2.1; iPhone8,1)"},
            };

        static private Dictionary<string, string> AuthenticationHeaders { get; set; } =
            new Dictionary<string, string>()
            {
                {"User-Agent", "PixivAndroidApp/5.0.64 (Android 6.0)"},
            };

        static public Dictionary<string, string> GetAuthHeaders(string hash = "")
        {
            var hashData = string.IsNullOrEmpty(hash) ? HashSecret : hash;
            string MD5Hash(string data)
            {
                if (string.IsNullOrEmpty(data)) return null;
                using (MD5 md5hash = MD5.Create())
                {
                    var res = md5hash.ComputeHash(Encoding.UTF8.GetBytes(data.Trim()));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < res.Length; i++)
                        builder.Append(res[i].ToString("x2"));
                    return builder.ToString();
                }
            }
            var time = DateTime.UtcNow.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:sszzz");
            var clientHash = MD5Hash(time + HashSecret);
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "X-Client-Time", time },
                { "X-Client-Hash", hashData }
            };
            return headers.Concat(AuthenticationHeaders).ToDictionary(x=>x.Key, y=>y.Value);
        }

        static private Dictionary<string, string> AuthorizeData { get; set; } =
            new Dictionary<string, string>()
            {
                {"client_id", "MOBrBDS8blbauoSck0ZfDbtuzpyT"},
                {"client_secret", "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj"},
                { "get_secure_url", "1"},
            };
        static public Dictionary<string, string> GetAuthorizeContent(GrantType type, Credentials credentials)
        {
            Dictionary<string, string> additionalValues = new Dictionary<string, string>(6);
            switch (type)
            {
                case GrantType.Password:
                    if (string.IsNullOrEmpty(credentials.Password))
                        throw new UnauthorizedAccessException("Empty Password");
                    if (string.IsNullOrEmpty(credentials.Username))
                        throw new UnauthorizedAccessException("Empty Username");

                    additionalValues.Add("grant_type", "password");
                    additionalValues.Add("username", credentials.Username);
                    additionalValues.Add("password", credentials.Password);
                    break;

                case GrantType.RefreshToken:
                    if (string.IsNullOrEmpty(credentials.Tokens.RefreshToken))
                        throw new UnauthorizedAccessException("Empty refresh token");

                    additionalValues.Add("grant_type", "refresh_token");
                    additionalValues.Add("refresh_token", credentials.Tokens.RefreshToken);
                    break;
            }
            return additionalValues.Concat(AuthorizeData).ToDictionary(x => x.Key, y => y.Value);
        }
        //{"Accept-Language", "en-us"},
    }
}
