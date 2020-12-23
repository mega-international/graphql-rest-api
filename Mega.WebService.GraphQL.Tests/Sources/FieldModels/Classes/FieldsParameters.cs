using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mega.WebService.GraphQL.Tests.Sources.FieldModels.Classes
{
    public class FieldsParameters
    {
        public Dictionary<string, FieldParameters> ParametersByName { get; }

        public FieldsParameters() : this(new Dictionary<string, FieldParameters>())
        {}

        public FieldsParameters(Dictionary<string, FieldParameters> parametersByName)
        {
            ParametersByName = parametersByName;
        }

        public FieldParameters Get(string name)
        {
            if(ParametersByName.TryGetValue(name, out var value))
            {
                return value;
            }
            return null;
        }

        public void AddOrUpdate(string fieldName, string paramName, string paramValue)
        {
            if (ParametersByName.TryGetValue(fieldName, out var parameters))
            {
                parameters.AddOrUpdate(paramName, paramValue);
            }
            else
            {
                ParametersByName.Add(fieldName, new FieldParameters(fieldName, new Dictionary<string, string> { { paramName, paramValue } }));
            }
        }

        public void AddOrUpdate(FieldParameters parameters)
        {
            if(ParametersByName.TryGetValue(parameters.Name, out var _))
            {
                ParametersByName[parameters.Name] = parameters;
            }
            else
            {
                ParametersByName.Add(parameters.Name, parameters);
            }
        }

        public void Remove(string name)
        {
            ParametersByName.Remove(name);
        }

        public void Clear()
        {
            ParametersByName.Clear();
        }
    }
}
