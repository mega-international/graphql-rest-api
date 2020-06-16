using Hopex.Model.Abstractions;
using Mega.Macro.API;
using System.Collections.Generic;

namespace Hopex.Modules.GraphQL
{
    public class UserContext
    {
        public MegaRoot MegaRoot { get; set; }
        public IMegaRoot IRoot { get; set; }
        public string WebServiceUrl { get; set; }
        public SchemaReference Schema { get; set; }
        public Dictionary<string, string> Languages { get; set; }
    }
}
