using Mega.WebService.GraphQL.Tests.Models.Interfaces.Crud;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models.Crud
{
    public class CrudFragment : ICrudFragment
    {
        private readonly string _name;
        private readonly string _metaclassName;
        public List<ICrudOutput> Outputs { get; set; } = new List<ICrudOutput>();

        public CrudFragment(string name, string metaclassName)
        {
            _name = name;
            _metaclassName = metaclassName;
        }

        public ICrudOutput GetOutput()
        {
            return new CrudOutputFragment(_name);
        }
        public string Serialize()
        {
            var result = $"fragment {_name} on {_metaclassName} {{\n";
            foreach(var output in Outputs)
            {
                result += $"{output.Serialize()}\n";
            }
            result += "}\n";
            return result;
        }
    }
}
