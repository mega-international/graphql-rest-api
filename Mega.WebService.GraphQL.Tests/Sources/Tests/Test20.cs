using Mega.WebService.GraphQL.Tests.Sources.FieldModels;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    public class Test20 : AbstractTest
    {
        private readonly Dictionary<string, string> _idByName = new Dictionary<string, string>();

        public Test20(Parameters parameters) : base(parameters) { }

        protected override void Initialisation()
        {
            _requester = new GraphQLRequester($"{_myServiceUrl}/api/{(IsAsyncMode ? "async/" : "")}/{_schemaAudit}");
        }

        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            SetConfig(EnvironmentId, RepositoryIdTo, ProfileId);
            await DeleteItemsAsync();
            await CreateItemsAsync();
            await LinkItemsAsync();
            await DeleteCascadeAsync();
            await CheckDeletedAsync();
        }

        private async Task DeleteItemsAsync()
        {
            Dictionary<string, List<JObject>> itemsByMetaclass = await GetItemsByMetaclassAsync();
            foreach(var items in itemsByMetaclass)
            {
                var metaclassName = items.Key;
                foreach (var item in items.Value)
                {
                    await DeleteOne(metaclassName, item.GetValue("id").ToString(), false);
                }
            }
        }

        private async Task<Dictionary<string, List<JObject>>> GetItemsByMetaclassAsync()
        {
            var itemsByMetaclass = new Dictionary<string, List<JObject>>
                {
                    { "Plan", new List<JObject>()},
                    { "Audit", new List<JObject>()},
                    { "AuditActivity", new List<JObject>()},
                    { "PersonSystem", new List<JObject>()}
                };
            foreach(var items in itemsByMetaclass)
            {
                var metaclassName = items.Key;
                var filterVal = new JObject
                {
                    { "name_in", GetNames(metaclassName) }
                };
                var inputs = new List<Field>
                {
                    new ListField("name_in", new ScalarField(null, "string"))
                };
                var outputs = new List<Field>
                {
                    new ScalarField("id", "string"),
                    new ScalarField("name", "string")
                };
                var result = await GetAll(metaclassName, outputs);
                foreach(var item in result)
                {
                    items.Value.Add(item as JObject);
                }
            }
            return itemsByMetaclass;
        }

        private JArray GetNames(string metaclassName)
        {
            switch(metaclassName)
            {
                case "Plan":
                {
                    return new JArray { "Audit Plan 2011" };
                }
                case "Audit":
                {
                    return new JArray { "Human Resource Audit: France", "Subsidiary audit: Japan" };
                }
                case "AuditActivity":
                {
                    return new JArray { "Audit Activity 11 a 1" };
                }
                case "PersonSystem":
                {
                    return new JArray { "Ernesto", "Francois", "Auditee 1" };
                }
                default:
                {
                    return null;
                }
            }
        }

        private async Task CreateItemsAsync()
        {
            await CreateItemAsync("plan", "Audit Plan 2011");

            await CreateItemAsync("audit", "Human Resource Audit: France");
            await CreateItemAsync("audit", "Subsidiary audit: Japan");

            await CreateItemAsync("auditActivity", "Audit Activity 11 a 1");

            await CreateItemAsync("personSystem", "Ernesto");
            await CreateItemAsync("personSystem", "Francois");
            await CreateItemAsync("personSystem", "Auditee 1");
        }

        private async Task CreateItemAsync(string metaclassName, string itemName)
        {
            var firstChar = metaclassName[0];
            var remaining = metaclassName.Substring(1);

            var queryName = "create" + char.ToUpper(firstChar) + remaining;
            var fieldName = char.ToLower(firstChar) + remaining;

            var queryStr =
                @"mutation
                {
                    " + queryName + $"({fieldName}: {{ name: \"{itemName}\" }})" + @"
                    {
                        id
                        name
                    }
                }";
            var result = await ProcessRawQuery(queryStr);
            _idByName.Add(result[queryName]["name"].ToString(), result[queryName]["id"].ToString());
        }

        private async Task LinkItemsAsync()
        {
            await LinkChildsToParentAsync("audit", "auditActivity", "Human Resource Audit: France",
                new List<string> { "Audit Activity 11 a 1" });

            await LinkChildsToParentAsync("audit", "auditorinAudit_PersonSystem", "Subsidiary audit: Japan",
                new List<string> { "Ernesto", "Francois" });

            await LinkChildsToParentAsync("audit", "leadAuditor_PersonSystem", "Subsidiary audit: Japan",
                new List<string> { "Francois" });

            await LinkChildsToParentAsync("audit", "mainAuditee_PersonSystem", "Subsidiary audit: Japan",
                new List<string> { "Auditee 1" });

            await LinkChildsToParentAsync("plan", "audit", "Audit Plan 2011",
                new List<string>{ "Human Resource Audit: France", "Subsidiary audit: Japan" });
        }

        private async Task LinkChildsToParentAsync(string parentClassName, string linkName, string parentName, List<string> childsNames)
        {
            var firstChar = parentClassName[0];
            var remaining = parentClassName.Substring(1);

            var queryName = "update" + char.ToUpper(firstChar) + remaining;
            var fieldName = char.ToLower(firstChar) + remaining;
            string linkList = "";
            foreach(var childName in childsNames)
            {
                if(linkList != "")
                {
                    linkList += ", ";
                }
                linkList += $"{{id: \"{_idByName[childName]}\"}}";
            }
            var queryStr =
            @"mutation
            {
                " + queryName + $"({fieldName}:" + @"
                {
                    " + linkName + @":
                    {
                        action: ADD
                        list: [" + linkList + @"]
                    }
                }, id: """ + _idByName[parentName] + @""")
                {
                    id
                }
            }";
            await ProcessRawQuery(queryStr);
        }

        private async Task DeleteCascadeAsync()
        {
            await DeleteOne("plan", _idByName["Audit Plan 2011"], true);
        }

        private async Task CheckDeletedAsync()
        {
            var deletedCount = 0;
            deletedCount += (await CheckItemDeletedAsync("plan", "Audit Plan 2011")) ? 1 : 0;

            deletedCount += (await CheckItemDeletedAsync("audit", "Human Resource Audit: France")) ? 1 : 0;
            deletedCount += (await CheckItemDeletedAsync("audit", "Subsidiary audit: Japan")) ? 1 : 0;

            deletedCount += (await CheckItemDeletedAsync("auditActivity", "Audit Activity 11 a 1")) ? 1 : 0;

            CountedStep("Deletion score", deletedCount, 4);
        }

        private async Task<bool> CheckItemDeletedAsync(string metaclassName, string itemName)
        {
            var queryStr =
            @"query
            {
                n1: " + metaclassName + $"(filter: {{ id: \"{_idByName[itemName]}\"}})" + @"
                {
                    id
                    name
                }
            }";
            var result = await ProcessRawQuery(queryStr);
            var resultArr = result["n1"] as JArray;
            if(resultArr.Count > 0)
            {
                var infos = $"Remaining items from metaclass {metaclassName}:<br>";
                foreach (var remaining in resultArr)
                {
                    infos += $"id: {remaining["id"]}, name: {remaining["name"]}<br>";
                }
                DetailedStep(infos);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
