using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hopex.Modules.GraphQL
{
    public class InputArguments
    {
        [Required]
        public string query;
        public Dictionary<string, object> variables;
    }
}
