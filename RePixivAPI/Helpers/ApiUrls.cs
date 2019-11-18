using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePixivAPI.Helpers
{
    public static class ApiUrls
    {
        public static string AuthUrl { get; private set; } = "https://oauth.secure.pixiv.net/auth/token";
    }
}
