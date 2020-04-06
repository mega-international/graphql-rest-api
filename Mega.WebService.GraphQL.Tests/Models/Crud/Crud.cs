using GraphQL.Common.Request;
using Mega.WebService.GraphQL.Tests.Models.Interfaces.Crud;
using Mega.WebService.GraphQL.Tests.Sources;
using Mega.WebService.GraphQL.Tests.Sources.FieldModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Models.Crud
{
    public class Crud : ICrud
    {
        private readonly GraphQLRequester _requester;
        public Crud(GraphQLRequester requester)
        {
            _requester = requester;
        }

        public async Task<ICrudResult> Query(string queryType, List<ICrudOutput> outputs, List<ICrudFragment> fragments, bool asyncMode)
        {
            var query = $"{queryType}{{\n";
            for(int i=1; i<=outputs.Count; ++i)
            {
                query += $"n{i}: {outputs[i].Serialize()}";
            }
            query += "}\n";
            foreach(var fragment in fragments)
            {
                query += fragment.Serialize();
            }
            var graphQLRequest = new GraphQLRequest
            {
                Query = query
            };
            var response = await _requester.SendPostAsync(graphQLRequest, asyncMode);
            return response.Data;
        }

        public ICrudOutput Create(string metaclassName, ICrudInput input, string mode, List<ICrudOutput> outputs)
        {
            var upperCaseName = char.ToUpper(metaclassName[0]).ToString() + metaclassName.Substring(1);
            var lowerCaseName = char.ToLower(metaclassName[0]).ToString() + metaclassName.Substring(1);

            CrudOutput output = new CrudOutput($"update{upperCaseName}")
            {
                Inputs = new List<ICrudInput>
                {
                    new CrudInput("creationMode", new JValue(mode), new EnumField("creationMode")),
                    input
                },
                Outputs = outputs
            };
            return output;
        }

        public ICrudOutput Read(string metaclassName, List<ICrudInput> inputs, List<ICrudOutput> outputs)
        {
            metaclassName = char.ToLower(metaclassName[0]).ToString() + metaclassName.Substring(1);

            //name
            var name = $"update{metaclassName}";
            CrudOutput output = new CrudOutput(name)
            {
                Inputs = inputs,
                Outputs = outputs
            };

            return output;
        }

        public ICrudOutput Update(string metaclassName, string id, ICrudInput input, List<ICrudOutput> outputs)
        {
            var upperCaseName = char.ToUpper(metaclassName[0]).ToString() + metaclassName.Substring(1);

            CrudOutput output = new CrudOutput($"update{upperCaseName}")
            {
                Inputs = new List<ICrudInput>
                {
                    new CrudInput("id", new JValue(id), new ScalarField("id", "String")),
                    input
                },
                Outputs = outputs
            };
            return output;
        }

        public ICrudOutput Delete(string metaclassName, string id, bool cascade)
        {
            var upperCaseName = char.ToUpper(metaclassName[0]).ToString() + metaclassName.Substring(1);
            var lowerCaseName = char.ToLower(metaclassName[0]).ToString() + metaclassName.Substring(1);

            CrudOutput output = new CrudOutput($"update{upperCaseName}")
            {
                Inputs = new List<ICrudInput>
                {
                    new CrudInput("id", new JValue(id), new ScalarField("id", "String")),
                    new CrudInput("cascade", new JValue(cascade), new ScalarField("cascade", "boolean"))
                },
                Outputs = new List<ICrudOutput>
                {
                    new CrudOutput("id")
                }
            };
            return output;
        }
    }
}
