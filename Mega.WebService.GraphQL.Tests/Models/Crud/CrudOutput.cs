using Mega.WebService.GraphQL.Tests.Models.Interfaces.Crud;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models.Crud
{
    public class CrudOutput : ICrudOutput
    {
        protected readonly string _name;
        public List<ICrudInput> Inputs { get; set; } = null;
        public List<ICrudOutput> Outputs { get; set; } = null;
        public CrudOutput(string name)
        {
            _name = name;
        }
        public virtual string Serialize()
        {
            var result = $"{_name}";
            if((Inputs?.Count ?? 0) > 0)
            {
                result += "(\n";
                for (int i = 0; i < Inputs.Count; ++i)
                {
                    if (i > 0)
                    {
                        result += ",\n";
                    }
                    result += Inputs[i].Serialize();
                }
                result += ")";
            }
            result += "\n";
            if((Outputs?.Count ?? 0) > 0)
            {
                result += "{\n";
                foreach (var output in Outputs)
                {
                    result += output.Serialize();
                }
                result += "}\n";
            }
            return result;
        }
    }
}
