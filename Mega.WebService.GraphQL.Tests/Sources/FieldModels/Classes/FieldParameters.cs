using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mega.WebService.GraphQL.Tests.Sources.FieldModels.Classes
{
    public class FieldParameters
    {
        public string Name { get; }
        public Dictionary<string, string> Parameters { get; }

        public FieldParameters(string name) : this(name, new Dictionary<string, string>()){ }

        public FieldParameters(string name, Dictionary<string, string> parameters)
        {
            Name = name;
            Parameters = parameters;
        }

        public string Get(string name)
        {
            return Parameters[name];
        }

        public void AddOrUpdate(string name, string value)
        {
            if(Parameters.TryGetValue(name, out var _))
            {
                Parameters[name] = value;
            }
            else
            {
                Parameters.Add(name, value);
            }
        }

        public void Remove(string name)
        {
            Parameters.Remove(name);
        }

        public void ClearParameters()
        {
            Parameters.Clear();
        }
    }
}
