using Mega.WebService.GraphQL.Tests.Sources.FieldModels;
using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    public class Test17 : AbstractTest
    {
        private string _compteRendu = "";
        public Test17(Parameters parameters) : base(parameters) { }

        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            //set source repository for the whole test
            SetConfig(EnvironmentId, RepositoryIdFrom, ProfileId);
            var fields = await GetFilterSchema("Application");
            RemoveUndesirable(ref fields);
            await TestFilters("Application", fields);
        }

        protected void RemoveUndesirable(ref List<Field> fields)
        {
            fields.RemoveAll(field => field.Name == "and" ||
                                    field.Name == "or" ||
                                    field.Name.StartsWith(Metaclasses.MetaFieldNames.externalIdentifier) ||
                                    field.Name.EndsWith("_some"));
        }

        protected async Task<List<Field>> GetFilterSchema(string metaclassName)
        {
            var kinds = Kind.Scalar | Kind.Enum | Kind.List;
            var filterName = metaclassName + "Filter";
            return await GetFieldsRequest(filterName, true, kinds);
        }

        protected async Task<List<JObject>> GetAllItems(string metaclassName, List<Field> filterFields)
        {
            var outputs = new List<Field>();
            foreach(var field in filterFields)
            {
                if(field.IsOriginalName())
                {
                    outputs.Add(field);
                }
            }
            return (await GetAll(metaclassName, outputs)).ToObject<List<JObject>>();
        }

        protected async Task TestFilters(string metaclassName, List<Field> fields)
        {
            _compteRendu = "";
            var items = await GetAllItems(metaclassName, fields);
            foreach(var field in fields)
            {
                try
                {
                    await TestFilter(metaclassName, items, field);
                }
                catch(Exception exception)
                {
                    _compteRendu += $"Field: {field.Name}\n";
                    _compteRendu += $"{exception.Message}\n";
                    _compteRendu += "\n\n\n";
                }
            }
        }

        protected async Task TestFilter(string metaclassName, List<JObject> items, Field field)
        {
            JToken filterFieldVal = field.GenerateValueFilter(items, out int expectedCount);
            var filterVal = new JObject
            {
                { field.Name, filterFieldVal }
            };
            var inputs = new List<Field> { field };
            try
            {
                var count = await GetCountByFilter(metaclassName, filterVal, inputs);
                var succeed = count == expectedCount;
                if(succeed == false)
                {
                    _compteRendu += $"Field: {field.Name}, value generated: {filterFieldVal.ToString()}, expected: {expectedCount}, count, {count}\n";
                    _compteRendu += "\n\n\n";
                }
            }
            catch(Exception exception)
            {
                _compteRendu += $"Field: {field.Name}, value generated: {filterFieldVal.ToString()}, expected: {expectedCount}\n";
                _compteRendu += $"{exception.Message}\n";
                _compteRendu += "\n\n\n";
            }
        }

        protected async Task<int> GetCountByFilter(string metaclassName, JObject filterVal, List<Field> inputs)
        {
            var outputs = new List<Field> { new ScalarField(MetaFieldNames.id, "String") };
            var arrItems = await GetFiltered(metaclassName, filterVal, inputs, outputs);
            return arrItems.Count;
        }
    }
}
