using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RePixivAPI.Objects;

namespace RePixivAPI.ApiUrls
{
    public class BasePixivApiService<T> where T: new()
    {

        protected async Task<T> ProjectAsync(HttpResponseMessage data)
        {
            return await ProjectAsync(await data.Content.ReadAsStringAsync());
        }

        protected async Task<T> ProjectAsync(string data)
        {
            return await Task.Run(()=>JsonConvert.DeserializeObject<T>(data));
        }

        protected async Task<T> ProjectResponseAsync(HttpResponseMessage data)
        {
            return await ProjectResponseAsync(await data.Content.ReadAsStringAsync());
        }

        protected async Task<T> ProjectResponseAsync(string data)
        {
            return await Task.Run(()=>JsonConvert.DeserializeObject<Response<T>>(data).response);
        }

    }
}
