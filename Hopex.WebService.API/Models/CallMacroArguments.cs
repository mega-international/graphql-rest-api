using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hopex.WebService.API.Models
{
    public class CallMacroArguments<T>
    {
        public string UserData { get; set; }

        public RequestArgument Request { get; set; }

        public CallMacroArguments(string path, T userData)
        {
            UserData = JsonConvert.SerializeObject(userData);
            Request = new RequestArgument
            {
                Path = path
            };
        }
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class RequestArgument
    {
        public IDictionary<string, string[]> Headers { get; set; }
        public string Path { get; set; }
    }
}
