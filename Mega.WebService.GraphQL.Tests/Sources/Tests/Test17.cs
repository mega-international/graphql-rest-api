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
            SetConfig(EnvironmentId, RepositoryIdTo, ProfileId);
            var fields = await GetFilterSchema("Application");
            RemoveUndesirable(ref fields);
            await TestFilters("Application", fields);
        }

        protected void RemoveUndesirable(ref List<Field> fields)
        {
            fields.RemoveAll(field => field.Name == "and" ||
                                    field.Name == "or" ||
                                    field.Name.StartsWith(Metaclasses.MetaFieldNames.externalIdentifier) ||
                                    field.Name.StartsWith("order") ||
                                    field.Name.StartsWith("link") ||
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
            JToken filterFieldVal = field.GenerateValueFilter(items, out var expected);
            if(filterFieldVal == null)
            {
                return;
            }
            var filterVal = new JObject
            {
                { field.Name, filterFieldVal }
            };
            var inputs = new List<Field> { field };
            try
            {
                var succeed = await CheckExpected(metaclassName, filterVal, inputs, expected);
            }
            catch(Exception exception)
            {
                _compteRendu += $"filterValue: {filterVal.ToString()}\n";
                _compteRendu += $"{exception.Message}\n";
                _compteRendu += "\n\n\n";
            }
        }

        protected async Task<bool> CheckExpected(string metaclassName, JObject filterVal, List<Field> inputs, List<JObject> expected)
        {
            var outputs = new List<Field> { new ScalarField(MetaFieldNames.id, "String") };
            var itemsFiltered = (await GetFiltered(metaclassName, filterVal, inputs, outputs)).ToObject<List<JObject>>();
            var succeed = true;
            if(itemsFiltered.Count == expected.Count)
            {
                foreach(var expectedItem in expected)
                {
                    var found = itemsFiltered.Find(itemFiltered => itemFiltered.GetValue(MetaFieldNames.id).ToString() == expectedItem.GetValue(MetaFieldNames.id).ToString());
                    if(found == null)
                    {
                        _compteRendu += $"Input: {filterVal}, expected item not found: {expectedItem}\n";
                        succeed = false;
                    }
                    else
                    {
                        itemsFiltered.Remove(found);
                    }
                }
                foreach(var itemFiltered in itemsFiltered)
                {
                    _compteRendu += $"Input: {filterVal}, this item must not appear: {itemFiltered}\n";
                }
            }
            else
            {
                var list1 = new List<JObject>(expected.Count > itemsFiltered.Count ? expected : itemsFiltered);
                var list2 = new List<JObject>(expected.Count < itemsFiltered.Count ? expected : itemsFiltered);
                list1.RemoveAll(it1 => list2.Exists(it2 => it1.GetValue("id").ToString() == it2.GetValue("id").ToString()));
                _compteRendu += $"Input: {filterVal}, expected count: {expected.Count}, count, {itemsFiltered.Count}\n";

                succeed = false;
            }
            return succeed;
        }
    }
}
