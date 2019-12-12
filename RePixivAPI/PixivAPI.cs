using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RePixivAPI.Helpers.AuthenticationConstants;

namespace RePixivAPI
{
    public class PixivAPI: PixivAPICore
    {
        public static async Task<PixivAPICore> GetPixivApiClientAsync(GrantType grantType, Credentials credentials)
        {
            PixivAPI client = new PixivAPI();
            await client.Authorize(grantType, credentials);
            return client;
        }
        private PixivAPI():base() { }
    }
}
