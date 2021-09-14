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
        public Dictionary<string, IMegaObject> Languages { get; set; }
        public IDictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();
            var properties = GetType().GetProperties();
            foreach (var property in properties)
            {
                dictionary.Add(property.Name, property.GetValue(this));
            }
            return dictionary;
        }
    }
}
