using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mega.WebService.GraphQL.Models
{
    public class CallMacroArguments<T>
    {
        public CallMacroArguments() { }

        public CallMacroArguments(string path, T userData)
        {
            UserData = JsonConvert.SerializeObject(userData);
            Request = new RequestArgument
            {
                Path = path
            };
        }

        public CallMacroArguments(IDictionary<string, string []> headers, string path, T userData)
        {
            UserData = JsonConvert.SerializeObject(userData);
            Request = new RequestArgument
            {
                Headers = headers,
                Path = path
            };
        }

        public string UserData { get; set; }
               
        public RequestArgument Request { get; set; }

        public T GetUserData()
        {
            return JsonConvert.DeserializeObject<T>(UserData);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class RequestArgument
    {
        public IDictionary<string, string []> Headers { get; set; }
        public string Path { get; set; }
    }
}
