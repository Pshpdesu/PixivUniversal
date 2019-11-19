using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePixivAPI.Helpers
{
    public static class ApiUrls
    {
        public static Uri AuthUrl { get; private set; } = new Uri("https://oauth.secure.pixiv.net/auth/token");
    }
}
