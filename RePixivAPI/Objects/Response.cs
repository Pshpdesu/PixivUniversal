using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RePixivAPI.Objects
{
    class Response<T>
    {
        [JsonProperty]
        public T response;
    }
}
