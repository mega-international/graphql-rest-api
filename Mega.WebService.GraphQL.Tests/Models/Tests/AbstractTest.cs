using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Mega.WebService.GraphQL.Tests.Models.FakeDatas;
using Mega.WebService.GraphQL.Tests.Models.FieldModels;
using Mega.WebService.GraphQL.Tests.Models.Metaclasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Models.Tests
{
    public abstract class AbstractTest
    {
        public class Result
        {
            public string MessageTime { get; set; } = "";
            public string MessageCounts { get; set; } = "";
            public string MessageDetails { get; set; } = "";
        }

        public class Parameters
        {
            [JsonProperty("environmentId")]
            public string EnvironmentId { get; set; }

            [JsonProperty("repositoryIdSource")]
            public string RepositoryIdFrom { get; set; }

            [JsonProperty("repositoryIdTarget")]
            public string RepositoryIdTo { get; set; }

            [JsonProperty("profileId")]
            public string ProfileId { get; set; }
        }

        protected Result testResult;
        protected GraphQLRequester _Requester;
        protected readonly HttpClient _HttpClient = new HttpClient();
        protected readonly Parameters parameters;
        protected readonly string myServiceUrl = "http://localhost/HOPEXGraphQL";
        protected readonly string uasUrl = "http://W-JMV/UAS";
        protected readonly string schemaITPM = "ITPM";
        protected readonly string schemaAudit = "Audit";
        protected readonly int maxArgsSize = 30;
        protected TimeMessageManager timeMessageManager = new TimeMessageManager();
        static protected readonly bool asyncMode = true;

        static protected readonly bool fakeMode = false;
        static protected readonly Container containerFrom = fakeMode ? new Container(true) : null;
        static protected readonly Container containerTo = fakeMode ? new Container(false) : null;
        protected Container currentContainer = null;

        protected string EnvironmentId => parameters.EnvironmentId;
        protected string RepositoryIdFrom => parameters.RepositoryIdFrom;
        protected string RepositoryIdTo => parameters.RepositoryIdTo;
        protected string ProfileId => parameters.ProfileId;


        protected AbstractTest(Parameters parameters)
        {
            this.parameters = parameters;
            Initialisation();
        }

        protected virtual void Initialisation()
        {
            _Requester = new GraphQLRequester($"{myServiceUrl}/api/{(asyncMode ? "async" : "")}/{schemaITPM}");
        }

        protected abstract void Steps(ITestParam oTestParam);

        public Result Run(ITestParam oTestParam)
        {
            //Initialisation
            testResult = new Result();
            timeMessageManager.Reset();

            //Run steps of test
            TimedStep("Full test", Steps, oTestParam);

            //End of test
            testResult.MessageTime = timeMessageManager.BuildMessage();
            return testResult;
        }

        protected void SetConfig(string env, string repo, string profile)
        {
            if(fakeMode)
            {
                currentContainer = repo == RepositoryIdFrom ? containerFrom : containerTo;
            }
            _Requester.EnvironmentId = env;
            _Requester.RepositoryId = repo;
            _Requester.ProfileId = profile;
            _Requester.UpdateHeader();
        }

        protected void CreateAndSetToken()
        {
            string token = GetToken();
            _Requester.SetToken(token);
        }

        protected string GetToken()
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            Task<HttpResponseMessage> response;
            string strResponse, token;

            string login = "scr";
            string password = "Hopex";
            fields.Add("grant_type", "password");
            fields.Add("scope", "hopex openid read write");
            fields.Add("username", login);
            fields.Add("password", password);
            fields.Add("client_id", "HopexAPI");
            fields.Add("client_secret", "secret");
            fields.Add("environmentId", EnvironmentId);

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{uasUrl}/connect/token"),
                Content = new FormUrlEncodedContent(fields)
            };
            response = _HttpClient.SendAsync(httpRequestMessage);
            strResponse = response.Result.Content.ReadAsStringAsync().Result;
            token = JsonConvert.DeserializeObject<JObject>(strResponse)?.GetValue("access_token").ToString() ?? "";
            return token;
        }

        protected List<Field> GetFieldsRequest(string tableName, string kindField, Kind flags = Kind.All)
        {
            //repetitive "ofType" is to get the "non null" wrapper type: example list = [elem!]! (non null array containing non null elements) (we need from 1 to 4 "kind" to handle types)
            GraphQLRequest request = new GraphQLRequest()
            {
                Query =
                @"{
                    __type(name: """ + tableName + @""")
                    {
                        " + kindField + @"
                        {
                            name
                            type
                            {
                                name
                                kind
                                ofType
                                {
                                    name
                                    kind
                                    ofType
                                    {
                                        name
                                        kind
                                        ofType
                                        {
                                            name
                                            kind
                                        }
                                    }
                                }
                            }
                        }
                    }
                }"
            };
            JArray array;
            if(fakeMode)
            {
                array = currentContainer.GetFields(tableName, kindField);
            }
            else
            {
                GraphQLResponse response = _Requester.SendPostAsync(request, asyncMode).Result;
                switch(kindField)
                {
                    case "fields":
                        array = response.Data.__type.fields;
                        break;
                    case "inputFields":
                        array = response.Data.__type.inputFields;
                        break;
                    default:
                        throw new ArgumentException("kindField has incorrect value");
                }
            }
            
            List<Field> fields = new List<Field>();
            foreach(dynamic elem in array)
            {
                Field GetField(dynamic elementType, string elementName)
                {
                    Kind kind = Field.GetKindByName((string)(elementType.kind));
                    bool nullable = kind != Kind.NonNull;
                    if(!nullable)
                    {
                        elementType = elementType.ofType;
                        kind = Field.GetKindByName((string)(elementType.kind));
                    }

                    if(flags.HasFlag(kind))
                    {
                        switch(kind)
                        {
                            case Kind.Scalar:
                                return new ScalarField(elementName, (string)elementType.name, nullable);

                            case Kind.Enum:
                                return new EnumField(elementName, nullable);

                            case Kind.Object:
                            case Kind.Interface:
                                return new ObjectField(elementName, new List<Field>(), nullable);

                            case Kind.List:
                                Field subfield = GetField(elementType.ofType, (string)elementType.name);
                                return new ListField(elementName, subfield, nullable);
                        }
                    }
                    return null;
                }
                Field field = GetField(elem.type, elem.name);
                if(field != null)
                    fields.Add(field);
            }
            return fields;
        }

        private JToken GetResponse(string query)
        {
            GraphQLRequest request = new GraphQLRequest()
            {
                Query = query
            };
            GraphQLResponse response = _Requester.SendPostAsync(request, asyncMode).Result;
            return response.Data;
        }

        private List<string> BuildOutputs(List<Field> outputFields)
        {
            List<string> outputs = new List<string>();
            outputFields.ForEach(field => outputs.Add(field.GetOutputFormat()));
            return outputs;
        }

        protected JArray GetAll(string metaclassName, List<Field> outputFields)
        {
            if(fakeMode)
            {
                return currentContainer.GetAll(metaclassName, outputFields);
            }

            //prepare datas
            string queryName = char.ToLowerInvariant(metaclassName [0]) + metaclassName.Substring(1);

            //outputs
            List<string> outputs = BuildOutputs(outputFields);

            //query
            string query = GraphQLRequester.GenerateRequestNoArg(QueryType.Query, queryName, outputs, metaclassName);
            return GetResponse(query) ["n1"] as JArray;
        }

        protected JArray GetOne(string metaclassName, string id, List<Field> outputFields)
        {
            //prepare datas
            string queryName = char.ToLowerInvariant(metaclassName [0]) + metaclassName.Substring(1);

            //inputs
            Dictionary<string, string> inputs = new Dictionary<string, string>()
            {
                { MetaFieldNames.id, $"\"{id}\"" }
            };

            //outputs
            List<string> outputs = BuildOutputs(outputFields);

            //query
            string query = GraphQLRequester.GenerateRequestOneArg(QueryType.Query, queryName, inputs, outputs, metaclassName);
            return GetResponse(query) ["n1"] as JArray;
        }

        protected List<JObject> CreateMulti(string metaclassName, List<JObject> objs, List<Field> inputFields, List<Field> outputFields)
        {
            if(fakeMode)
            {
                return currentContainer.CreateMulti(metaclassName, objs, inputFields, outputFields);
            }

            //prepare datas
            List<JObject> metaClassListObjects = new List<JObject>();
            string inputName = char.ToLowerInvariant(metaclassName [0]) + metaclassName.Substring(1);
            ObjectField objField = new ObjectField(metaclassName, inputFields, false);

            //inputs
            List<Dictionary<string, string>> inputsList = new List<Dictionary<string, string>>();
            objs.ForEach(obj =>
            {
                string strElement = objField.GetStringFormat(obj);
                Dictionary<string, string> inputs = new Dictionary<string, string>()
                {
                    { inputName, strElement }
                };
                inputsList.Add(inputs);
            });

            //outputs
            List<string> outputs = BuildOutputs(outputFields);

            //Requests by packs
            int limit = maxArgsSize <= 0 ? objs.Count : maxArgsSize;
            for(int start = 0;start < objs.Count;start += limit)
            {
                int length = Math.Min(limit, objs.Count - start);

                //query
                string query = GraphQLRequester.GenerateRequestMultiArgs(QueryType.Mutation, $"create{metaclassName}", inputsList.GetRange(start, length), outputs, metaclassName);
                JToken array = GetResponse(query);

                //result
                for(int idx = 1;idx <= length;++idx)
                {
                    JObject newElement = array [$"n{idx}"] as JObject;
                    metaClassListObjects.Add(newElement);
                }
            }
            return metaClassListObjects;
        }

        protected JObject CreateOne(string metaclassName, JObject obj, List<Field> inputFields, List<Field> outputFields)
        {
            //prepare datas
            string inputName = char.ToLowerInvariant(metaclassName [0]) + metaclassName.Substring(1);
            ObjectField objField = new ObjectField(metaclassName, inputFields, false);
            string strElement = objField.GetStringFormat(obj);

            //inputs
            Dictionary<string, string> inputs = new Dictionary<string, string>()
            {
                { inputName, strElement }
            };

            //outputs
            List<string> outputs = BuildOutputs(outputFields);

            //query
            string query = GraphQLRequester.GenerateRequestOneArg(QueryType.Mutation, $"create{metaclassName}", inputs, outputs, metaclassName);
            return GetResponse(query) ["n1"] as JObject;
        }

        protected List<JObject> UpdateMulti(string metaclassName, List<Tuple<string, JObject>> args, List<Field> inputFields, List<Field> outputFields)
        {
            if(fakeMode)
            {
                return currentContainer.UpdateMulti(metaclassName, args, inputFields, outputFields);
            }

            //prepare datas
            List<JObject> metaClassListObjects = new List<JObject>();
            string inputName = char.ToLowerInvariant(metaclassName [0]) + metaclassName.Substring(1);
            ObjectField objField = new ObjectField(metaclassName, inputFields, false);

            //inputs
            List<Dictionary<string, string>> inputsList = new List<Dictionary<string, string>>();
            args.ForEach(arg =>
            {
                string strElement = objField.GetStringFormat(arg.Item2);
                Dictionary<string, string> inputs = new Dictionary<string, string>()
                {
                    { MetaFieldNames.id, $"\"{arg.Item1}\"" },
                    { inputName, strElement }
                };
                inputsList.Add(inputs);
            });

            //outputs
            List<string> outputs = BuildOutputs(outputFields);

            //Requests by packs
            int limit = maxArgsSize <= 0 ? args.Count : maxArgsSize;
            for(int start = 0;start < args.Count;start += limit)
            {
                int length = Math.Min(limit, args.Count - start);

                //query
                string query = GraphQLRequester.GenerateRequestMultiArgs(QueryType.Mutation, $"update{metaclassName}", inputsList.GetRange(start, length), outputs, metaclassName);
                JToken array = GetResponse(query);

                //result
                for(int idx = 1;idx <= length;++idx)
                {
                    JObject newElement = array [$"n{idx}"] as JObject;
                    metaClassListObjects.Add(newElement);
                }
            }
            return metaClassListObjects;
        }

        protected JObject UpdateOne(string metaclassName, Tuple<string, JObject> arg, List<Field> inputFields, List<Field> outputFields)
        {
            //prepare datas
            string inputName = char.ToLowerInvariant(metaclassName [0]) + metaclassName.Substring(1);
            ObjectField objField = new ObjectField(metaclassName, inputFields, false);
            string strElement = objField.GetStringFormat(arg.Item2);

            //inputs
            Dictionary<string, string> inputs = new Dictionary<string, string>()
            {
                { MetaFieldNames.id, $"\"{arg.Item1}\"" },
                { inputName, strElement }
            };

            //outputs
            List<string> outputs = BuildOutputs(outputFields);

            //query
            string query = GraphQLRequester.GenerateRequestOneArg(QueryType.Mutation, $"update{metaclassName}", inputs, outputs, metaclassName);
            return GetResponse(query) ["n1"] as JObject;
        }

        protected void DeleteMulti(string metaclassName, List<Tuple<string, bool>> args)
        {
            if(fakeMode)
            {
                currentContainer.DeleteMulti(metaclassName, args);
            }
            //inputs
            List<Dictionary<string, string>> inputsList = new List<Dictionary<string, string>>();
            args.ForEach(arg =>
            {
                string strCascade = arg.Item2 ? "true" : "false";
                Dictionary<string, string> inputs = new Dictionary<string, string>()
                {
                    { MetaFieldNames.id, $"\"{arg.Item1}\"" },
                    { "cascade", $"{strCascade}" }
                };
                inputsList.Add(inputs);
            });

            //Requests by packs
            int limit = maxArgsSize <= 0 ? args.Count : maxArgsSize;
            for(int start = 0;start < args.Count;start += limit)
            {
                int length = Math.Min(limit, args.Count - start);

                //query
                string query = GraphQLRequester.GenerateRequestMultiArgs(QueryType.Mutation, $"delete{metaclassName}", inputsList.GetRange(start, length), new List<string>() { MetaFieldNames.id }, metaclassName);
                GetResponse(query);
            }
        }

        protected void DeleteOne(string metaclassName, Tuple<string, bool> arg)
        {
            //Prepare data
            string strCascade = arg.Item2 ? "true" : "false";

            //inputs
            Dictionary<string, string> inputs = new Dictionary<string, string>()
            {
                { MetaFieldNames.id, $"\"{arg.Item1}\"" },
                { "cascade", $"{strCascade}" }
            };

            //query
            string query = GraphQLRequester.GenerateRequestOneArg(QueryType.Mutation, $"delete{metaclassName}", inputs, new List<string>() { MetaFieldNames.id }, metaclassName);
            GetResponse(query);
        }

        protected string FormatElementIdName(string id, string name)
        {
            return $"{name} (id: {id})";
        }

        protected TResult TimedStep<TResult>(string stepName, Func<TResult> function)
        {
            return (TResult)TimedStep(stepName, function as Delegate);
        }

        protected TResult TimedStep<T1, TResult>(string stepName, Func<T1, TResult> function, T1 param1)
        {
            return (TResult)TimedStep(stepName, function as Delegate, param1);
        }

        protected TResult TimedStep<T1, T2, TResult>(string stepName, Func<T1, T2, TResult> function, T1 param1, T2 param2)
        {
            return (TResult)TimedStep(stepName, function as Delegate, param1, param2);
        }

        protected TResult TimedStep<T1, T2, T3, TResult>(string stepName, Func<T1, T2, T3, TResult> function, T1 param1, T2 param2, T3 param3)
        {
            return (TResult)TimedStep(stepName, function as Delegate, param1, param2, param3);
        }

        protected TResult TimedStep<T1, T2, T3, T4, TResult>(string stepName, Func<T1, T2, T3, T4, TResult> function, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            return (TResult)TimedStep(stepName, function as Delegate, param1, param2, param3, param4);
        }

        protected TResult TimedStep<T1, T2, T3, T4, T5, TResult>(string stepName, Func<T1, T2, T3, T4, T5, TResult> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            return (TResult)TimedStep(stepName, function as Delegate, param1, param2, param3, param4, param5);
        }

        protected void TimedStep(string stepName, Action action)
        {
            TimedStep(stepName, action as Delegate);
        }

        protected void TimedStep<T1>(string stepName, Action<T1> action, T1 param1)
        {
            TimedStep(stepName, action as Delegate, param1);
        }

        protected void TimedStep<T1, T2>(string stepName, Action<T1, T2> action, T1 param1, T2 param2)
        {
            TimedStep(stepName, action as Delegate, param1, param2);
        }

        protected void TimedStep<T1, T2, T3>(string stepName, Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
        {
            TimedStep(stepName, action as Delegate, param1, param2, param3);
        }

        protected void TimedStep<T1, T2, T3, T4>(string stepName, Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            TimedStep(stepName, action as Delegate, param1, param2, param3, param4);
        }

        protected void TimedStep<T1, T2, T3, T4, T5>(string stepName, Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            TimedStep(stepName, action as Delegate, param1, param2, param3, param4, param5);
        }

        protected object TimedStep(string stepName, Delegate function, params object[] parameters)
        {
            //Pre step
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            timeMessageManager.Start(stepName);

            //Step
            object result = function.DynamicInvoke(parameters);

            //Post step
            stopwatch.Stop();
            timeMessageManager.End(stopwatch.Elapsed.ToString());
            return result;
        }

        protected void CountedStep(string stepName, int count)
        {
            testResult.MessageCounts += $"<p>{stepName}: {count}</p>";
        }

        protected void DetailedStep(string stepDetails)
        {
            testResult.MessageDetails += $"<p>{stepDetails}</p>";
        }

        protected MetaClass GetMetaClass(string metaclassName)
        {
            //Choose selected test
            MetaClass metaclass = (MetaClass)(Activator.CreateInstance(Type.GetType($"Mega.WebService.GraphQL.Tests.Models.Metaclasses.{metaclassName}")));
            return metaclass;
        }

        protected virtual (List<Field>, List<Field>) GetInAndOutFields(MetaClass metaclass)
        {
            Kind flags = Kind.Scalar | Kind.Enum;
            var fieldsOut = GetFieldsRequest(metaclass.Name, "fields", flags);
            var fieldsIn = GetFieldsRequest("Input" + metaclass.Name, "inputFields", flags);
            return (fieldsIn, fieldsOut);
        }

        protected virtual void MetaclassSchema(MetaClass metaclass)
        {
            //set source repository
            SetConfig(EnvironmentId, RepositoryIdFrom, ProfileId);

            //get schema
            (metaclass.InputFields, metaclass.Fields) = GetInAndOutFields(metaclass);

            //filter
            List<string> blackListFields = metaclass.GetBlackListedFields();
            metaclass.InputFields.RemoveAll(field => blackListFields.Exists(blackListed => blackListed == field.Name));
            metaclass.Fields.RemoveAll(field => blackListFields.Exists(blackListed => blackListed == field.Name));
        }

        protected (List<JObject>, List<JObject>) Copy(MetaClass metaclass)
        {
            //Set to source repository
            SetConfig(EnvironmentId, RepositoryIdFrom, ProfileId);

            //Get schema fields
            var inputFields = new List<Field>(metaclass.InputFields);
            var outputFields = new List<Field>(metaclass.Fields);

            //Get all data from first repository
            var originals = TimedStep($"Get all {metaclass.GetPluralName(true)} from first repository", GetAll, metaclass.Name, outputFields).ToObject<List<JObject>>();

            //update external id
            foreach(JObject original in originals)
            {
                JProperty extIdProp = original.Property(MetaFieldNames.externalIdentifier);
                extIdProp.Value = original.GetValue(MetaFieldNames.id);
            }

            //Create elements into the second one
            SetConfig(EnvironmentId, RepositoryIdTo, ProfileId);
            var newDatas = originals.Count > 0 ? TimedStep($"Create all {metaclass.GetPluralName(true)} to second repository", CreateMulti, metaclass.Name, originals, inputFields, outputFields)
                                                         : new List<JObject>();

            //Add message
            CountedStep($"Number of {metaclass.GetPluralName(true)} copied", newDatas.Count);
            string messageDetails = $"List of {metaclass.GetPluralName(true)} created:<br>";
            newDatas.ForEach(newData =>
            {
                string originalId = newData.GetValue(MetaFieldNames.externalIdentifier).ToString();
                string newId = newData.GetValue(MetaFieldNames.id).ToString();
                string name = newData.GetValue(MetaFieldNames.name).ToString();
                messageDetails += $"{name}: {originalId} => {newId}<br>";
            });
            messageDetails += "<br>";
            DetailedStep(messageDetails);

            //return new datas
            return (originals, newDatas);
        }

        protected void Link(List<JObject> originalBaseDatas, Dictionary<string, JObject> newBaseDatas, Dictionary<string, string> originalToNewLinkedDatasId, MetaClass metaclassBase, MetaClass metaclassLinked)
        {
            var metaclassBaseName = metaclassBase.Name;
            var metaclassLinkedName = metaclassLinked.Name;
            string linkFieldName = metaclassBase.GetFieldNameFromLinkedMetaClass(metaclassLinkedName);

            List<Tuple<string, JObject>> args = new List<Tuple<string, JObject>>();
            foreach(JObject originalBaseData in originalBaseDatas)
            {
                //Get newBaseData from original one
                var originalBaseDataId = originalBaseData.GetValue(MetaFieldNames.id).ToString();
                var newBaseData = newBaseDatas [originalBaseDataId];

                //Build array of linked datas attached to base data
                var linkedDatasArray = originalBaseData.GetValue(linkFieldName) as JArray;
                if(linkedDatasArray.Count > 0)
                {
                    var linkedDatasIdsArray = new JArray();
                    foreach(JObject linkedData in linkedDatasArray)
                    {
                        var originalLinkedDataId = linkedData.GetValue(MetaFieldNames.id).ToString();
                        var newLinkedDataId = originalToNewLinkedDatasId [originalLinkedDataId];
                        linkedDatasIdsArray.Add(new JValue(newLinkedDataId));
                    }

                    //Prepare new values to set
                    var linkedDataInput = new JObject
                    {
                        { "action", new JValue("ADD") },
                        { "list", linkedDatasIdsArray }
                    };

                    //Set new values for newBaseData
                    var linkedDataProp = newBaseData.Property(linkFieldName);
                    if(linkedDataProp is null)
                    {
                        newBaseData.Add(linkFieldName, linkedDataInput);
                    }
                    else
                    {
                        linkedDataProp.Value = linkedDataInput;
                    }

                    Tuple<string, JObject> arg = new Tuple<string, JObject>(newBaseData.GetValue(MetaFieldNames.id).ToString(), newBaseData);
                    args.Add(arg);
                }
            }

            //Build inputFields
            List<Field> inputFields = new List<Field>()
            {
                new ObjectField(linkFieldName, new List<Field>
                {
                    new EnumField("action", true),
                    new ListField("list", new ScalarField(null, "String", false), false)
                })
            };

            //Build outputFields
            List<Field> outputFields = new List<Field>()
            {
                new ScalarField(MetaFieldNames.id, "String", false),
                new ScalarField(MetaFieldNames.name, "String", false),
                new ListField(linkFieldName, new ObjectField(null, new List<Field>
                {
                    new ScalarField(MetaFieldNames.id, "String", false),
                    new ScalarField(MetaFieldNames.name, "String", false),
                }))
            };

            //Update base datas to add links to linked datas
            var updatedBaseDatas = TimedStep($"Add links between {metaclassBase.GetPluralName(true)} and {metaclassLinked.GetPluralName(true)}", UpdateMulti, metaclassBaseName, args, inputFields, outputFields);

            //Add messages
            int linksCount = 0;
            string messageDetails = "";
            updatedBaseDatas.ForEach(updatedBaseData =>
            {
                string newBaseDataId = updatedBaseData.GetValue(MetaFieldNames.id).ToString();
                string newBaseDataName = updatedBaseData.GetValue(MetaFieldNames.name).ToString();
                messageDetails += $"{metaclassBase.GetSingleName()} {FormatElementIdName(newBaseDataId, newBaseDataName)} is attached to {metaclassLinked.GetPluralName(true)}:<br>";
                JArray attachedLinkedDatas = updatedBaseData.GetValue(linkFieldName) as JArray;
                linksCount += attachedLinkedDatas.Count;
                foreach(var attachedLinkedData in attachedLinkedDatas)
                {
                    string newLinkedDataId = attachedLinkedData [MetaFieldNames.id].ToString();
                    string newLinkedDataName = attachedLinkedData [MetaFieldNames.name].ToString();
                    string space = "&nbsp;&nbsp;";
                    messageDetails += $"{space}- {FormatElementIdName(newLinkedDataId, newLinkedDataName)}<br>";
                }
            });
            CountedStep("Number of links created", linksCount);
            DetailedStep(messageDetails);
        }

        protected void DeleteLinks(MetaClass metaclassBase, MetaClass metaclassLinked)
        {
            var metaclassBaseName = metaclassBase.Name;
            var metaclassLinkedName = metaclassLinked.Name;
            string linkFieldName = metaclassBase.GetFieldNameFromLinkedMetaClass(metaclassLinkedName);

            string stepDetails = "List of links removed:<br>";
            int linksCount = 0;

            //Get all baseDatas
            //outputs
            List<Field> outputs = new List<Field>()
            {
                new ScalarField(MetaFieldNames.id, "String", false),
                new ScalarField(MetaFieldNames.name, "String", false),
                new ListField(linkFieldName,
                    new ObjectField(null,
                        new List<Field>()
                        {
                            new ScalarField(MetaFieldNames.id, "String", false),
                            new ScalarField(MetaFieldNames.name, "String", false),
                        }), false)
            };
            //query
            JArray baseDatasArray = TimedStep($"Get {metaclassBase.GetPluralName(true)}' {metaclassLinked.GetPluralName(true)}", GetAll, metaclassBaseName, outputs);

            //Update baseDatas to remove links
            //inputs
            List<Field> inputs = new List<Field>()
            {
                new ObjectField(linkFieldName,
                    new List<Field>()
                    {
                        new EnumField("action", false),
                        new ListField("list", new ScalarField(null, "String", false))
                    }, false)
            };
            //arguments
            List<Tuple<string, JObject>> args = new List<Tuple<string, JObject>>();
            foreach(var baseData in baseDatasArray)
            {
                string baseDataId = baseData [MetaFieldNames.id].ToString();
                string baseDataName = baseData [MetaFieldNames.name].ToString();
                JArray linkedDatas = baseData [linkFieldName] as JArray;
                if(linkedDatas.Count > 0)
                {
                    //Initialisation of fields
                    JObject inputBaseData = new JObject();
                    JObject inputLinkedData = new JObject();
                    JArray inputList = new JArray();
                    foreach(var linkedData in linkedDatas)
                    {
                        string linkedDataId = linkedData [MetaFieldNames.id].ToString();
                        string linkedDataName = linkedData [MetaFieldNames.name].ToString();
                        string link = $"{FormatElementIdName(baseDataId, baseDataName)} <===> {FormatElementIdName(linkedDataId, linkedDataName)}";
                        inputList.Add(linkedData [MetaFieldNames.id]);
                        stepDetails += $"{link}<br>";
                        ++linksCount;
                    }

                    //Build structure
                    inputLinkedData.Add("action", new JValue("REMOVE"));
                    inputLinkedData.Add("list", inputList);
                    inputBaseData.Add(linkFieldName, inputLinkedData);
                    Tuple<string, JObject> arg = new Tuple<string, JObject>(baseDataId, inputBaseData);
                    args.Add(arg);
                }
            }

            //query
            if(args.Count > 0)
            {
                TimedStep($"Links removal for each {metaclassBase.GetSingleName(true)}", UpdateMulti, metaclassBaseName, args, inputs, outputs);
            }
            CountedStep("Number of links removed:", linksCount);
            DetailedStep(stepDetails);
        }

        protected void DeleteAllFromMetaClass(MetaClass metaclass)
        {
            var metaclassName = metaclass.Name;

            string stepDetails = $"List of {metaclass.GetPluralName(true)}:<br>";

            //Get all item for metaClass
            var metaClassItems = TimedStep($"Get all {metaclass.GetPluralName(true)}", GetAll, metaclassName,
                                        new List<Field>()
                                        {
                                            new ScalarField(MetaFieldNames.id, "String", false),
                                            new ScalarField(MetaFieldNames.name, "String", false),
                                        }).ToObject<List<JObject>>();

            //Delete all metaClassElement
            if(metaClassItems.Count > 0)
            {
                List<Tuple<string, bool>> args = new List<Tuple<string, bool>>();
                void AddItemToDelete(JObject item)
                {
                    string id = item.GetValue(MetaFieldNames.id).ToString();
                    string name = item.GetValue(MetaFieldNames.name).ToString();
                    stepDetails += $"{FormatElementIdName(id, name)}<br>";
                    args.Add(new Tuple<string, bool>(id, false));
                }
                metaClassItems.ForEach(AddItemToDelete);
                TimedStep($"Delete all {metaclass.GetPluralName(true)}", DeleteMulti, metaclassName, args);
            }

            DetailedStep(stepDetails);
            CountedStep($"Number of {metaclass.GetPluralName(true)} deleted", metaClassItems.Count);
        }

        protected void Compare(MetaClass metaclass, List<JObject> originals)
        {
            var metaclassName = metaclass.Name;
            var inputFields = new List<Field>(metaclass.InputFields);
            var outputFields = new List<Field>(metaclass.Fields);

            SetConfig(EnvironmentId, RepositoryIdTo, ProfileId);
            Dictionary<string, JObject> createdDatasById = new Dictionary<string, JObject>();
            {
                var createdDatas = TimedStep($"Get all {metaclass.GetPluralName(true)} from second repository", GetAll, metaclassName, outputFields).ToObject<List<JObject>>();
                createdDatas.ForEach(createdData => createdDatasById.Add(createdData [MetaFieldNames.externalIdentifier].ToString(), createdData));
            }

            void CompareAll()
            {
                bool ok = true;
                int compareCount = 0;
                foreach(var original in originals)
                {
                    var originalId = original.GetValue(MetaFieldNames.id).ToString();
                    var createdData = createdDatasById [originalId];
                    bool compare = Compare(original, createdData, inputFields);
                    ok = ok && compare;
                    compareCount += compare ? 1 : 0;
                }
                CountedStep($"Number of {metaclass.GetPluralName(true)} copied successfully", compareCount);
            }
            TimedStep($"Compare all {metaclass.GetPluralName(true)} between repositories", CompareAll);
        }

        protected bool Compare(JObject original, JObject created, List<Field> fields)
        {
            bool ok = true;
            int compareCount = 0;
            int compareCountTotal = 0;
            string messageDetails = "Next comparison:<br>";
            fields.ForEach(field =>
            {
                if(field.Name == MetaFieldNames.externalIdentifier)
                {
                    return;
                }
                var value1 = original.GetValue(field.Name);
                var value2 = created.GetValue(field.Name);
                bool compare = JToken.EqualityComparer.Equals(value1, value2);
                ok = ok && compare;
                string messageValues = $"{field.Name}: {value1.ToString()} <===> {value2.ToString()}";
                messageDetails += $"{ColoredMessageFromBool(messageValues, compare)}<br>";
                ok = ok && compare;
                ++compareCountTotal;
                compareCount += compare ? 1 : 0;
            });
            string messageResult = $"Comparison score: {compareCount}/{compareCountTotal}";
            messageDetails += $"{ColoredMessageFromBool(messageResult, ok)}<br>";
            DetailedStep(messageDetails);
            return ok;
        }

        protected string ColoredMessageFromBool(string message, bool ok)
        {
            return $"<span style=\"color: {(ok ? "green" : "red")}\">{message}</span>";
        }

        protected Dictionary<string, JObject> GetDatasByExternalId(List<JObject> datas)
        {
            var result = new Dictionary<string, JObject>();
            datas.ForEach(data => result.Add(data[MetaFieldNames.externalIdentifier].ToString(), data));
            return result;
        }

        protected Dictionary<string, string> GetDatasIdByExternalId(List<JObject> datas)
        {
            var result = new Dictionary<string, string>();
            datas.ForEach(data => result.Add(data [MetaFieldNames.externalIdentifier].ToString(), data [MetaFieldNames.id].ToString()));
            return result;
        }

        protected void TransferTest(MetaClass metaclass)
        {
            //Generate token
            CreateAndSetToken();

            //Initialisation of metaclasses
            MetaclassSchema(metaclass);

            //Transfer part
            var (originals, _) = TimedStep($"Copy all {metaclass.GetSingleName(true)}", Copy, metaclass);

            //Comparison part
            TimedStep($"Compare all {metaclass.GetSingleName(true)} between source and target repositories", Compare, metaclass, originals);
        }

        protected void LinkTransferTest(MetaClass metaclass1, MetaClass metaclass2)
        {
            //Generate token
            CreateAndSetToken();

            //Set source repository
            SetConfig(EnvironmentId, RepositoryIdFrom, ProfileId);

            //Get original datas for class1
            var collectionFieldName = metaclass1.GetFieldNameFromLinkedMetaClass(metaclass2.Name);
            var outputFields = new List<Field>
            {
                new ScalarField(MetaFieldNames.id, "String", false),
                new ListField(collectionFieldName,
                    new ObjectField(null, new List<Field>
                    {
                        new ScalarField(MetaFieldNames.id, "String", false)
                    }, false), false)
            };
            var originals1 = TimedStep($"Get all {metaclass1.GetSingleName(true)} from source repository", GetAll, metaclass1.Name, outputFields).ToObject<List<JObject>>();

            //Set source repository
            SetConfig(EnvironmentId, RepositoryIdTo, ProfileId);

            //Get created datas for class1
            outputFields = new List<Field>
            {
                new ScalarField(MetaFieldNames.id, "String", false),
                new ScalarField(MetaFieldNames.externalIdentifier, "String", false)
            };
            var created1 = TimedStep($"Get all {metaclass1.GetSingleName(true)} from target repository", GetAll, metaclass1.Name, outputFields).ToObject<List<JObject>>();
            var createdByExtId1 = GetDatasByExternalId(created1);

            //Get created datas for class2
            outputFields.Add(new ScalarField(MetaFieldNames.id, "String", false));
            var created2 = TimedStep($"Get all {metaclass2.GetSingleName(true)} from target repository", GetAll, metaclass2.Name, outputFields).ToObject<List<JObject>>();
            var createdIdByExtId2 = GetDatasIdByExternalId(created2);

            //Links
            TimedStep($"Create links between {metaclass1.GetPluralName(true)} and {metaclass2.GetPluralName(true)}", Link, originals1, createdByExtId1, createdIdByExtId2, metaclass1, metaclass2);
        }

        protected void DeletionTest(MetaClass metaclass)
        {
            //Set token and config
            CreateAndSetToken();
            SetConfig(EnvironmentId, RepositoryIdTo, ProfileId);

            //Delete all datas
            TimedStep($"Deletion of {metaclass.GetPluralName(true)}", DeleteAllFromMetaClass, metaclass);
        }

        protected void LinkDeletionTest(MetaClass metaclass1, MetaClass metaclass2)
        {
            //Set token and config
            CreateAndSetToken();
            SetConfig(EnvironmentId, RepositoryIdTo, ProfileId);

            //Delete links
            TimedStep($"Deletion of links between {metaclass1.GetPluralName(true)} and {metaclass2.GetPluralName(true)}", DeleteLinks, metaclass1, metaclass2);
        }
    }
}
