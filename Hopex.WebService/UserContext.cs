using Hopex.Model.Abstractions;
using Mega.Macro.API;

namespace Hopex.Modules.GraphQL
{
    public class UserContext
    {
        public MegaRoot MegaRoot { get; set; }
        public IMegaRoot IRoot { get; set; }
        public string WebServiceUrl { get; set; }
        public string SchemaFile { get; set; }
    }
}
