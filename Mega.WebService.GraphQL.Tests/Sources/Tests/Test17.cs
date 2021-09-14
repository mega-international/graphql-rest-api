using Mega.WebService.GraphQL.Tests.Sources.FieldModels;
using Mega.WebService.GraphQL.Tests.Sources.FieldModels.Classes;
using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    public class Test17 : AbstractTest
    {
        private readonly Dictionary<string, Field> _originalFieldByName = new Dictionary<string, Field>();
        public Test17(Parameters parameters) : base(parameters) { }

        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            //set source repository for the whole test
            SetConfig(Destination);
            var fields = await GetFilterSchema("Application");
            ArrangeFields(ref fields);
            await TestFilters("Application", fields);
        }

        protected void ArrangeFields(ref List<Field> fields)
        {
            fields.RemoveAll(field =>
                                    field.Name == "and" ||
                                    field.Name == "or" ||
                                    field.Name.StartsWith(Metaclasses.MetaFieldNames.externalIdentifier) ||
                                    field.Name.StartsWith("order") ||
                                    field.Name.StartsWith("link") ||
                                    field.Name.EndsWith("_some") ||
                                    field.Name.StartsWith("currentState") ||
                                    field.Name.StartsWith("currentWorkflowStatus") ||
                                    field.Name.StartsWith("creator") ||
                                    field.Name.StartsWith("modifier"));
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
                    _originalFieldByName.Add(field.Name, field);
                }
            }
            var parameters = new FieldsParameters();
            parameters.AddOrUpdate("comment", "format", "RAW");
            return (await GetAll(metaclassName, outputs, parameters)).ToObject<List<JObject>>();
        }

        protected async Task TestFilters(string metaclassName, List<Field> fields)
        {
            var items = await GetAllItems(metaclassName, fields);
            foreach(var field in fields)
            {
                try
                {
                    await TestFilter(metaclassName, items, field);
                }
                catch(Exception exception)
                {
                    var message = $"Exception thrown for field: {field.Name}<br>";
                    message += $"{exception.ToString()}<br>";
                    DetailedStep(message);
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
            try
            {
                var succeed = await CheckExpected(metaclassName, filterVal, field, expected);
            }
            catch(Exception exception)
            {
                var message = $"Exception thrown for filterValue: {filterVal.ToString()}<br>";
                message += $"{exception.ToString()}<br>";
                DetailedStep(message);
            }
        }

        protected async Task<bool> CheckExpected(string metaclassName, JObject filterVal, Field field, List<JObject> expected)
        {
            var outputs = new List<Field>
            {
                new ScalarField(MetaFieldNames.id, "String"),
                _originalFieldByName[field.GetOriginalName()]
            };
            var parameters = new FieldsParameters();
            parameters.AddOrUpdate("comment", "format", "RAW");
            var filtered = (await GetFiltered(metaclassName, filterVal, new List<Field> { field }, outputs, parameters)).ToObject<List<JObject>>();
            return CompareLists(field, filterVal[field.Name], filtered, expected);
        }

        private bool CompareLists(Field field, JToken value, List<JObject> filtered, List<JObject> expected)
        {
            RemoveCommon(ref filtered, ref expected);
            CountedStep($"Property name: {field.Name}, value: { value.ToString()}<br>Missing items count", expected.Count, 0);
            CountedStep($"Property name: {field.Name}, value: { value.ToString()}<br>Extra items count", filtered.Count, 0);
            if (filtered.Count == 0 && expected.Count == 0)
            {
                return true;
            }
            //Missing
            var message = $"Property name: {field.Name}, value: {value.ToString()}<br>";
            message += ReportList(filtered, field, "Extra items");
            message += ReportList(expected, field, "Missing items");
            DetailedStep(message);
            return false;
        }

        private string ReportList(List<JObject> list, Field field, string label)
        {
            if (list.Count > 0)
            {
                var message = $"{label}:<br>";
                foreach (var item in list)
                {
                    var id = item.GetValue(MetaFieldNames.id).ToString();
                    var filteredValue = item.GetValue(field.GetOriginalName()).ToString();
                    message += $"  id: {id}, value: {filteredValue}<br>";
                }
                return message;
            }
            else
            {
                return "";
            }
        }

        private void RemoveCommon(ref List<JObject> list1, ref List<JObject> list2)
        {
            var idx1 = 0;
            while (idx1 < list1.Count)
            {
                var item1 = list1[idx1];
                var found = false;
                var idx2 = 0;
                while (idx2 < list2.Count)
                {
                    var item2 = list2[idx2];
                    if (IsEqual(item1, item2))
                    {
                        list1.RemoveAt(idx1);
                        list2.RemoveAt(idx2);
                        found = true;
                        break;
                    }
                    else
                    {
                        ++idx2;
                    }
                }
                if (!found)
                {
                    ++idx1;
                }
            }
        }

        private bool IsEqual(JObject item1, JObject item2)
        {
            var id1 = item1.GetValue(MetaFieldNames.id).ToString();
            var id2 = item2.GetValue(MetaFieldNames.id).ToString();
            return id1 == id2;
        }
    }
}
