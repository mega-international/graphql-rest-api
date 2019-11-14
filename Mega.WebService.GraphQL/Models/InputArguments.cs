using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mega.WebService.GraphQL.Models
{
    public class InputArguments
    {
        [Required]
        public string query;
        public Dictionary<string, object> variables;
    }
}
