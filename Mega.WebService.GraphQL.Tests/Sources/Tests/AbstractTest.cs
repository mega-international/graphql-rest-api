using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Mega.WebService.GraphQL.Tests.Models;
using Mega.WebService.GraphQL.Tests.Sources.FieldModels;
using Mega.WebService.GraphQL.Tests.Sources.FieldModels.Classes;
using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using Mega.WebService.GraphQL.Tests.Sources.Requesters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    public abstract class AbstractTest
    {
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

            [JsonProperty("source")]
            public object Source { get; set; }

            [JsonProperty("destination")]
            public object Destination { get; set; }

            [JsonProperty("synchronisation")]
            public string Synchronisation { get; set; }
        }

        protected ProgressionModel _progression;
        protected IRequester _requester;
        protected readonly string _myServiceUrl = ConfigurationManager.AppSettings["HopexGraphQL"].TrimEnd('/');
        protected const string _schemaITPM = "ITPM";
        protected const string _schemaAudit = "Audit";
        protected const string _schemaRisk = "Risk";
        protected const int _maxArgsSize = 30;
        protected TimeMessageManager timeMessageManager = new TimeMessageManager();

        protected ISessionInfos Source { get; set; }
        protected ISessionInfos Destination { get; set; }
        protected bool IsAsyncMode { get; set; }

        protected bool HASMode = ConfigurationManager.AppSettings["HASMode"] == "1";

        protected AbstractTest(Parameters parameters)
        {
            if(HASMode)
            {
                Source = JsonConvert.DeserializeObject<HASSessionInfos>(parameters.Source.ToString());
                Destination = JsonConvert.DeserializeObject<HASSessionInfos>(parameters.Destination.ToString());
            }
            else
            {
                Source = JsonConvert.DeserializeObject<SessionInfos>(parameters.Source.ToString());
                Destination = JsonConvert.DeserializeObject<SessionInfos>(parameters.Destination.ToString());
            }
            IsAsyncMode = parameters.Synchronisation == "async";
            Initialisation();
        }

        protected IRequester GenerateRequester(string uri)
        {
            if(HASMode)
            {
                return new HASGraphQLRequester(uri);
            }
            else
            {
                return new GraphQLRequester(uri);
            }
        }

        protected virtual void Initialisation()
        {
            _requester = GenerateRequester($"{_myServiceUrl}/api/{(IsAsyncMode ? "async/" : "")}{_schemaITPM}");
        }

        protected abstract Task StepsAsync(ITestParam oTestParam);

        public async Task<ProgressionModel> Run(ITestParam oTestParam, ProgressionModel progression)
        {
            //Initialisation
            _progression = progression;
            _progression.Reset();
            timeMessageManager.Reset();

            //Run steps of test
            try
            {
                await TimedStep("Full test", StepsAsync, oTestParam).ConfigureAwait(false);
                _progression.Status.Set(TestStatus.Completed);
            }
            catch(Exception exception)
            {
                _progression.Status.Set(TestStatus.Failure);
                _progression.Error.Set(new ExceptionContent(exception));
            }

            //End of test
            _progression.MessageTime.Set(timeMessageManager.BuildMessage());
            return _progression;
        }

        protected void SetConfig(IRequester requester, ISessionInfos sessionInfos)
        {
            requester.SetConfig(sessionInfos);
        }

        protected void SetConfig(ISessionInfos sessionInfos)
        {
            SetConfig(_requester, sessionInfos);
        }

        protected async Task<List<Field>> GetFieldsRequest(string tableName, bool input, Kind flags = Kind.All)
        {
            //repetitive "ofType" is to get the "non null" wrapper type: example list = [elem!]! (non null array containing non null elements) (we need from 1 to 4 "kind" to handle types)
            var kindField = input ? "inputFields" : "fields";
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
            GraphQLResponse response = await _requester.SendPostAsync(request, IsAsyncMode);
            JArray array = input ? response.Data.__type.inputFields : response.Data.__type.fields;
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

        protected async Task<JToken> ProcessRawQuery(IRequester requester, string query, CancellationToken token)
        {
            GraphQLRequest request = new GraphQLRequest()
            {
                Query = query
            };
            GraphQLResponse response;
            try
            {
                response = await requester.SendPostAsync(request, IsAsyncMode, token);
            }
            catch (Exception)
            {
                throw;
            }
            if(response == null)
            {
                throw new NullReferenceException($"Response GraphQL is null for query: {query}");
            }
            if ((response.Errors?.Length ?? 0) > 0)
            {
                var errorMessage = "";
                foreach(var error in response.Errors)
                {
                    errorMessage += $"Error: {error.Message} <br>";
                }
                throw new Exception(errorMessage);
            }
            return response.Data;
        }

        protected async Task<JToken> ProcessRawQuery(IRequester requester, string query)
        {
            return await ProcessRawQuery(requester, query, CancellationToken.None);
        }

        protected async Task<JToken> ProcessRawQuery(string query, CancellationToken token)
        {
            return await ProcessRawQuery(_requester, query, token);
        }

        protected async Task<JToken> ProcessRawQuery(string query)
        {
            return await ProcessRawQuery(_requester, query, CancellationToken.None);
        }

        private List<string> BuildOutputs(IReadOnlyList<Field> outputFields, FieldsParameters fieldsParameters = null)
        {
            List<string> outputs = new List<string>();
            foreach(var field in outputFields)
            {
                var fieldParameters = fieldsParameters?.Get(field.Name);
                outputs.Add(field.GetOutputFormat(true, fieldParameters));
            }
            return outputs;
        }

        protected string FirstCharUpper(string metaclassName)
        {
            if(metaclassName.Length == 0)
            {
                return "";
            }
            else if (metaclassName.Length == 1)
            {
                return metaclassName.ToUpper();
            }
            else
            {
                return char.ToUpper(metaclassName[0]) + metaclassName.Substring(1);
            }
        }

        protected async Task<JArray> GetAll(string metaclassName, IReadOnlyList<Field> outputFields)
        {
            return await GetAll(metaclassName, outputFields, null);
        }

        protected async Task<JArray> GetAll(string metaclassName, IReadOnlyList<Field> outputFields, FieldsParameters fieldsParameters)
        {
            metaclassName = FirstCharUpper(metaclassName);

            //prepare datas
            string queryName = char.ToLowerInvariant(metaclassName [0]) + metaclassName.Substring(1);

            //outputs
            List<string> outputs = BuildOutputs(outputFields, fieldsParameters);

            //query
            string query = GraphQLRequester.GenerateRequestNoArg(QueryType.Query, queryName, outputs, metaclassName);
            return (await ProcessRawQuery(query))["n1"] as JArray;
        }

        protected async Task<JArray> GetFiltered(string metaclassName, JObject filterVal, List<Field> inputFields, IReadOnlyList<Field> outputFields)
        {
            return await GetFiltered(metaclassName, filterVal, inputFields, outputFields, null);
        }

        protected async Task<JArray> GetFiltered(string metaclassName, JObject filterVal, List<Field> inputFields, IReadOnlyList<Field> outputFields, FieldsParameters fieldsParameters)
        {
            metaclassName = FirstCharUpper(metaclassName);

            if (filterVal == null || inputFields == null)
            {
                return await GetAll(metaclassName, outputFields);
            }

            //prepare datas
            string queryName = char.ToLowerInvariant(metaclassName [0]) + metaclassName.Substring(1);

            //inputs
            ObjectField fieldFilter = new ObjectField(MetaFieldNames.filter, inputFields);
            Dictionary<string, string> inputs = new Dictionary<string, string>()
            {
                { MetaFieldNames.filter, fieldFilter.GetStringFormat(filterVal) }
            };

            //outputs
            List<string> outputs = BuildOutputs(outputFields, fieldsParameters);

            //query
            string query = GraphQLRequester.GenerateRequestOneArg(QueryType.Query, queryName, inputs, outputs, metaclassName);
            try
            {
                return (await ProcessRawQuery(query))["n1"] as JArray;
            }
            catch(Exception)
            {
                return null;
            }
        }

        protected async Task<List<JObject>> CreateMulti(string metaclassName, List<JObject> objs, IReadOnlyList<Field> inputFields, IReadOnlyList<Field> outputFields)
        {
            metaclassName = FirstCharUpper(metaclassName);

            //prepare datas
            List<JObject> metaClassListObjects = new List<JObject>();
            string inputName = char.ToLowerInvariant(metaclassName [0]) + metaclassName.Substring(1);
            ObjectField objField = new ObjectField(metaclassName, new List<Field>(inputFields), false);

            //inputs
            List<Dictionary<string, string>> inputsList = new List<Dictionary<string, string>>();
            objs.ForEach(obj =>
            {
                string strElement = objField.GetStringFormat(obj);
                Dictionary<string, string> inputs = new Dictionary<string, string>()
                {
                    { inputName, strElement },
                    { "creationMode", "BUSINESS" }
                };
                inputsList.Add(inputs);
            });

            //outputs
            List<string> outputs = BuildOutputs(outputFields);

            //Requests by packs
            int limit = _maxArgsSize <= 0 ? objs.Count : _maxArgsSize;
            for(int start = 0;start < objs.Count;start += limit)
            {
                int length = Math.Min(limit, objs.Count - start);

                //query
                string query = GraphQLRequester.GenerateRequestMultiArgs(QueryType.Mutation, $"create{metaclassName}", inputsList.GetRange(start, length), outputs, metaclassName);
                JToken array = await ProcessRawQuery(query);

                //result
                for(int idx = 1;idx <= length;++idx)
                {
                    if(!(array [$"n{idx}"] is JObject newElement))
                    {
                        throw new NullReferenceException($"Creation failed for element n{idx}");
                    }
                    metaClassListObjects.Add(newElement);
                }
            }
            return metaClassListObjects;
        }

        protected async Task<JObject> CreateOne(string metaclassName, JObject obj, IReadOnlyList<Field> inputFields, IReadOnlyList<Field> outputFields)
        {
            metaclassName = FirstCharUpper(metaclassName);

            //prepare datas
            string inputName = char.ToLowerInvariant(metaclassName [0]) + metaclassName.Substring(1);
            ObjectField objField = new ObjectField(metaclassName, new List<Field>(inputFields), false);
            string strElement = objField.GetStringFormat(obj);

            //inputs
            Dictionary<string, string> inputs = new Dictionary<string, string>()
            {
                { inputName, strElement },
                { "creationMode", "BUSINESS" }
            };

            //outputs
            List<string> outputs = BuildOutputs(outputFields);

            //query
            string query = GraphQLRequester.GenerateRequestOneArg(QueryType.Mutation, $"create{metaclassName}", inputs, outputs, metaclassName);
            return (await ProcessRawQuery(query))["n1"] as JObject;
        }

        protected async Task<List<JObject>> UpdateMulti(string metaclassName, List<Tuple<string, JObject>> args, IReadOnlyList<Field> inputFields, IReadOnlyList<Field> outputFields)
        {
            metaclassName = FirstCharUpper(metaclassName);

            //prepare datas
            List<JObject> metaClassListObjects = new List<JObject>();
            string inputName = char.ToLowerInvariant(metaclassName [0]) + metaclassName.Substring(1);
            ObjectField objField = new ObjectField(metaclassName, new List<Field>(inputFields), false);

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
            int limit = _maxArgsSize <= 0 ? args.Count : _maxArgsSize;
            for(int start = 0;start < args.Count;start += limit)
            {
                int length = Math.Min(limit, args.Count - start);

                //query
                string query = GraphQLRequester.GenerateRequestMultiArgs(QueryType.Mutation, $"update{metaclassName}", inputsList.GetRange(start, length), outputs, metaclassName);
                JToken array = await ProcessRawQuery(query);

                //result
                for(int idx = 1;idx <= length;++idx)
                {
                    JObject newElement = array [$"n{idx}"] as JObject;
                    metaClassListObjects.Add(newElement);
                }
            }
            return metaClassListObjects;
        }

        protected async Task<JObject> UpdateOne(string metaclassName, Tuple<string, JObject> arg, IReadOnlyList<Field> inputFields, IReadOnlyList<Field> outputFields)
        {
            metaclassName = FirstCharUpper(metaclassName);

            //prepare datas
            string inputName = char.ToLowerInvariant(metaclassName [0]) + metaclassName.Substring(1);
            ObjectField objField = new ObjectField(metaclassName, new List<Field>(inputFields), false);
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
            return (await ProcessRawQuery(query))["n1"] as JObject;
        }

        protected async Task DeleteMulti(string metaclassName, List<Tuple<string, bool>> args)
        {
            metaclassName = FirstCharUpper(metaclassName);

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
            int limit = _maxArgsSize <= 0 ? args.Count : _maxArgsSize;
            for(int start = 0;start < args.Count;start += limit)
            {
                int length = Math.Min(limit, args.Count - start);

                //query
                string query = GraphQLRequester.GenerateRequestMultiArgs(QueryType.Mutation, $"delete{metaclassName}", inputsList.GetRange(start, length), new List<string>() { MetaFieldNames.id }, metaclassName);
                await ProcessRawQuery(query);
            }
        }

        protected async Task DeleteOne(string metaclassName, string id, bool cascade)
        {
            metaclassName = FirstCharUpper(metaclassName);

            //inputs
            Dictionary<string, string> inputs = new Dictionary<string, string>()
            {
                { MetaFieldNames.id, $"\"{id}\"" },
                { "cascade", $"{cascade.ToString().ToLower()}" }
            };

            //query
            string query = GraphQLRequester.GenerateRequestOneArg(QueryType.Mutation, $"delete{metaclassName}", inputs, new List<string>() { MetaFieldNames.id }, metaclassName);
            await ProcessRawQuery(query);
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

        protected TResult TimedStep<T1, T2, T3, T4, T5, T6, TResult>(string stepName, Func<T1, T2, T3, T4, T5, T6, TResult> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            return (TResult)TimedStep(stepName, function as Delegate, param1, param2, param3, param4, param5, param6);
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

        protected void TimedStep<T1, T2, T3, T4, T5, T6>(string stepName, Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            TimedStep(stepName, action as Delegate, param1, param2, param3, param4, param5, param6);
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

        protected async Task<TResult> TimedStepAsync<TResult>(string stepName, Func<Task<TResult>> function)
        {
            return await TimedStepAsync(stepName, function.Invoke());
        }

        protected async Task<TResult> TimedStepAsync<T1, TResult>(string stepName, Func<T1, Task<TResult>> function, T1 param1)
        {
            return await TimedStepAsync(stepName, function.Invoke(param1));
        }

        protected async Task<TResult> TimedStepAsync<T1, T2, TResult>(string stepName, Func<T1, T2, Task<TResult>> function, T1 param1, T2 param2)
        {
            return await TimedStepAsync(stepName, function.Invoke(param1, param2));
        }

        protected async Task<TResult> TimedStepAsync<T1, T2, T3, TResult>(string stepName, Func<T1, T2, T3, Task<TResult>> function, T1 param1, T2 param2, T3 param3)
        {
            return await TimedStepAsync(stepName, function.Invoke(param1, param2, param3));
        }

        protected async Task<TResult> TimedStepAsync<T1, T2, T3, T4, TResult>(string stepName, Func<T1, T2, T3, T4, Task<TResult>> function, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            return await TimedStepAsync(stepName, function.Invoke(param1, param2, param3, param4));
        }

        protected async Task<TResult> TimedStepAsync<T1, T2, T3, T4, T5, TResult>(string stepName, Func<T1, T2, T3, T4, T5, Task<TResult>> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            return await TimedStepAsync(stepName, function.Invoke(param1, param2, param3, param4, param5));
        }

        protected async Task<TResult> TimedStepAsync<T1, T2, T3, T4, T5, T6, TResult>(string stepName, Func<T1, T2, T3, T4, T5, T6, Task<TResult>> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            return await TimedStepAsync(stepName, function.Invoke(param1, param2, param3, param4, param5, param6));
        }

        protected async Task TimedStepAsync(string stepName, Func<Task> function)
        {
            await TimedStepAsync(stepName, function.Invoke());
        }

        protected async Task TimedStepAsync<T1>(string stepName, Func<T1, Task> function, T1 param1)
        {
            await TimedStepAsync(stepName, function.Invoke(param1));
        }

        protected async Task TimedStepAsync<T1, T2>(string stepName, Func<T1, T2, Task> function, T1 param1, T2 param2)
        {
            await TimedStepAsync(stepName, function.Invoke(param1, param2));
        }

        protected async Task TimedStepAsync<T1, T2, T3>(string stepName, Func<T1, T2, T3, Task> function, T1 param1, T2 param2, T3 param3)
        {
            await TimedStepAsync(stepName, function.Invoke(param1, param2, param3));
        }

        protected async Task TimedStepAsync<T1, T2, T3, T4>(string stepName, Func<T1, T2, T3, T4, Task> function, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            await TimedStepAsync(stepName, function.Invoke(param1, param2, param3, param4));
        }

        protected async Task TimedStepAsync<T1, T2, T3, T4, T5>(string stepName, Func<T1, T2, T3, T4, T5, Task> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            await TimedStepAsync(stepName, function.Invoke(param1, param2, param3, param4, param5));
        }

        protected async Task TimedStepAsync<T1, T2, T3, T4, T5, T6>(string stepName, Func<T1, T2, T3, T4, T5, T6, Task> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            await TimedStepAsync(stepName, function.Invoke(param1, param2, param3, param4, param5, param6));
        }

        protected async Task TimedStepAsync(string stepName, Task task)
        {
            //Pre step
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            timeMessageManager.Start(stepName);

            //Step
            await task;

            //Post step
            stopwatch.Stop();
            timeMessageManager.End(stopwatch.Elapsed.ToString());
        }

        protected async Task<TResult> TimedStepAsync<TResult>(string stepName, Task<TResult> task)
        {
            await TimedStepAsync(stepName, task as Task);
            return await task;
        }

        protected void CountedStep(string stepName, int count)
        {
            _progression.MessageCounts.Add($"<p>{stepName}: {count}</p>");
        }

        protected void CountedStep(string stepName, int count, int expected)
        {
            _progression.MessageCounts.Add(ColoredMessageFromBool($"<p>{stepName}: <br>current {count} / expected: {expected}</p>", count == expected));
        }

        protected void CountedStep(string stepName, int count, int target, Func<int, int, (string, bool)>compare)
        {
            var result = compare(count, target);
            _progression.MessageCounts.Add(ColoredMessageFromBool($"<p>{stepName}: <br>{result.Item1}</p>", result.Item2));
        }

        protected void DetailedStep(string stepDetails)
        {
            _progression.MessageDetails.Add($"<p>{stepDetails}</p>");
        }

        protected MetaClass GetMetaClass(string metaclassName)
        {
            //Choose selected test
            MetaClass metaclass = (MetaClass)(Activator.CreateInstance(Type.GetType($"Mega.WebService.GraphQL.Tests.Sources.Metaclasses.{metaclassName}")));
            return metaclass;
        }

        protected virtual async Task<(List<Field>, List<Field>)> GetInAndOutFields(MetaClass metaclass)
        {
            Kind flags = Kind.Scalar | Kind.Enum;
            var fieldsOut = await GetFieldsRequest(metaclass.Name, false, flags);
            var fieldsIn = await GetFieldsRequest("Input" + metaclass.Name, true, flags);
            return (fieldsIn, fieldsOut);
        }

        protected virtual async Task MetaclassSchema(MetaClass metaclass)
        {
            //set source repository
            SetConfig(Source);

            //get schema
            (metaclass.InputFields, metaclass.Fields) = await GetInAndOutFields(metaclass);

            //filter
            List<string> blackListFields = metaclass.GetBlackListedFields();
            metaclass.InputFields.RemoveAll(field => blackListFields.Exists(blackListed => blackListed == field.Name));
            metaclass.Fields.RemoveAll(field => blackListFields.Exists(blackListed => blackListed == field.Name));
        }

        protected void SetExternalIds(ref List<JObject> items)
        {
            foreach(var item in items)
            {
                JProperty extIdProp = item.Property(MetaFieldNames.externalIdentifier);
                if (extIdProp != null)
                {
                    extIdProp.Value = item.GetValue(MetaFieldNames.id);
                }
            }
        }

        protected void ClearExternalIds(ref List<JObject> items)
        {
            foreach(var item in items)
            {
                JProperty extIdProp = item.Property(MetaFieldNames.externalIdentifier);
                if (extIdProp != null)
                {
                    extIdProp.Value = new JValue("");
                }
            }
        }

        protected async Task<(List<JObject>, List<JObject>)> CopyAll(MetaClass metaclass)
        {
            return await CopyFiltered(metaclass, null, null);
        }

        protected async Task<(List<JObject>, List<JObject>)> CopyFiltered(MetaClass metaclass, JObject filter, List<Field> filterInputs)
        {
            //Set to source repository
            SetConfig(Source);

            //Get schema fields
            IReadOnlyList<Field> inputFields = metaclass.InputFields;
            IReadOnlyList<Field> outputFields = metaclass.Fields;

            //Get datas from first repository
            var originals = (await TimedStep($"Get {metaclass.GetPluralName(true)} from first repository", GetFiltered, metaclass.Name, filter, filterInputs, outputFields)).ToObject<List<JObject>>();

            //Set to destination repository
            SetConfig(Destination);

            //Create elements into the second one
            SetExternalIds(ref originals);
            var newDatas = await TimedStep($"Create all {metaclass.GetPluralName(true)} to second repository", CreateMulti, metaclass.Name, originals, inputFields, outputFields);
            ClearExternalIds(ref originals);

            //Add message
            WriteMessagesCreatedItems(metaclass, newDatas);

            //Get created datas into second repository
            newDatas = (await TimedStep($"Get all {metaclass.GetPluralName(true)} from second repository", GetAll, metaclass.Name, outputFields)).ToObject<List<JObject>>();

            //return new datas
            return (originals, newDatas);
        }

        protected void WriteMessagesCreatedItems(MetaClass metaclass, List<JObject> createdItems)
        {
            CountedStep($"Number of {metaclass.GetPluralName(true)} copied", createdItems.Count);
            var messageDetails = $"List of {metaclass.GetPluralName(true)} created:<br>";
            createdItems.ForEach(newData =>
            {
                var originalId = newData.GetValue(MetaFieldNames.externalIdentifier).ToString();
                var newId = newData.GetValue(MetaFieldNames.id).ToString();
                var name = newData.GetValue(MetaFieldNames.name).ToString();
                messageDetails += $"{name}: {originalId} => {newId}<br>";
            });
            messageDetails += "<br>";
            DetailedStep(messageDetails);
        }

        protected async Task Link(List<JObject> originalBaseDatas, Dictionary<string, JObject> newBaseDatas, Dictionary<string, string> originalToNewLinkedDatasId, MetaClass metaclassBase, MetaClass metaclassLinked, string linkName)
        {
            List<Tuple<string, JObject>> args = new List<Tuple<string, JObject>>();
            foreach(JObject originalBaseData in originalBaseDatas)
            {
                //Get newBaseData from original one
                var originalBaseDataId = originalBaseData.GetValue(MetaFieldNames.id).ToString();
                var newBaseData = newBaseDatas [originalBaseDataId];

                //Build array of linked datas attached to base data
                var linkedDatasArray = originalBaseData.GetValue(linkName) as JArray;
                if(linkedDatasArray.Count > 0)
                {
                    var linkedDatasIdsArray = new JArray();
                    foreach(JObject linkedData in linkedDatasArray)
                    {
                        var originalLinkedDataId = linkedData.GetValue(MetaFieldNames.id).ToString();
                        var newLinkedDataId = originalToNewLinkedDatasId [originalLinkedDataId];
                        linkedDatasIdsArray.Add(new JObject
                        {
                            { MetaFieldNames.id, new JValue(newLinkedDataId) }
                        });
                    }

                    //Prepare new values to set
                    var linkedDataInput = new JObject
                    {
                        { "action", new JValue("ADD") },
                        { "list", linkedDatasIdsArray }
                    };

                    //Set new values for newBaseData
                    var linkedDataProp = newBaseData.Property(linkName);
                    if(linkedDataProp is null)
                    {
                        newBaseData.Add(linkName, linkedDataInput);
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
                new ObjectField(linkName, new List<Field>
                {
                    new EnumField("action"),
                    new ListField("list",
                        new ObjectField(null, new List<Field>
                        {
                            new ScalarField(MetaFieldNames.id, "String")
                        })
                    )
                })
            };

            //Build outputFields
            List<Field> outputFields = new List<Field>()
            {
                new ScalarField(MetaFieldNames.id, "String", false),
                new ScalarField(MetaFieldNames.name, "String", false),
                new ListField(linkName, new ObjectField(null, new List<Field>
                {
                    new ScalarField(MetaFieldNames.id, "String", false),
                    new ScalarField(MetaFieldNames.name, "String", false),
                }))
            };

            //Update base datas to add links to linked datas
            var updatedBaseDatas = await TimedStep($"Add links {linkName} between {metaclassBase.GetPluralName(true)} and {metaclassLinked.GetPluralName(true)}",
                UpdateMulti, metaclassBase.Name, args, inputFields, outputFields);

            //Add messages
            int linksCount = 0;
            string messageDetails = "";
            updatedBaseDatas.ForEach(updatedBaseData =>
            {
                string newBaseDataId = updatedBaseData.GetValue(MetaFieldNames.id).ToString();
                string newBaseDataName = updatedBaseData.GetValue(MetaFieldNames.name).ToString();
                messageDetails += $"{metaclassBase.GetSingleName()} {FormatElementIdName(newBaseDataId, newBaseDataName)} is attached to {metaclassLinked.GetPluralName(true)}:<br>";
                JArray attachedLinkedDatas = updatedBaseData.GetValue(linkName) as JArray;
                linksCount += attachedLinkedDatas.Count;
                foreach(var attachedLinkedData in attachedLinkedDatas)
                {
                    string newLinkedDataId = attachedLinkedData [MetaFieldNames.id].ToString();
                    string newLinkedDataName = attachedLinkedData [MetaFieldNames.name].ToString();
                    string space = "&nbsp;&nbsp;";
                    messageDetails += $"{space}- {FormatElementIdName(newLinkedDataId, newLinkedDataName)}<br>";
                }
            });
            CountedStep($"Number of links {linkName} between {metaclassBase.GetPluralName(true)} and {metaclassLinked.GetPluralName(true)} created",
                linksCount);
            DetailedStep(messageDetails);
        }

        protected async Task DeleteLinks(MetaClass metaclassBase, string linkFieldName)
        {
            var stepDetails = "List of links removed:<br>";
            var linksCount = 0;

            //Get all baseDatas
            //outputs
            var outputs = new List<Field>()
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
            var baseDatasArray = await TimedStep($"Get {linkFieldName} from {metaclassBase.GetPluralName(true)}", GetAll, metaclassBase.Name, outputs);

            //Update baseDatas to remove links
            //inputs
            var inputs = new List<Field>()
            {
                new ObjectField(linkFieldName, new List<Field>
                {
                    new EnumField("action"),
                    new ListField("list",
                        new ObjectField(null, new List<Field>
                        {
                            new ScalarField(MetaFieldNames.id, "String")
                        })
                    )
                })
            };
            //arguments
            var args = new List<Tuple<string, JObject>>();
            foreach(var baseData in baseDatasArray)
            {
                var baseDataId = baseData [MetaFieldNames.id].ToString();
                var baseDataName = baseData [MetaFieldNames.name].ToString();
                var linkedDatas = baseData [linkFieldName] as JArray;
                if(linkedDatas.Count > 0)
                {
                    //Initialisation of fields
                    var inputBaseData = new JObject();
                    var inputLinkedData = new JObject();
                    var inputList = new JArray();
                    foreach(var linkedData in linkedDatas)
                    {
                        var linkedDataId = linkedData [MetaFieldNames.id].ToString();
                        var linkedDataName = linkedData [MetaFieldNames.name].ToString();
                        var link = $"{FormatElementIdName(baseDataId, baseDataName)} <===> {FormatElementIdName(linkedDataId, linkedDataName)}";
                        inputList.Add(new JObject()
                        {
                            { MetaFieldNames.id, linkedData [MetaFieldNames.id] }
                        });
                        stepDetails += $"{link}<br>";
                        ++linksCount;
                    }

                    //Build structure
                    inputLinkedData.Add("action", new JValue("REMOVE"));
                    inputLinkedData.Add("list", inputList);
                    inputBaseData.Add(linkFieldName, inputLinkedData);
                    var arg = new Tuple<string, JObject>(baseDataId, inputBaseData);
                    args.Add(arg);
                }
            }

            //query
            if(args.Count > 0)
            {
                await TimedStep($"Links removal for each {metaclassBase.GetSingleName(true)}", UpdateMulti, metaclassBase.Name, args, inputs, outputs);
            }
            CountedStep($"Number of {linkFieldName} from {metaclassBase.GetPluralName(true)} removed", linksCount);
            DetailedStep(stepDetails);
        }

        protected async Task DeleteAllFromMetaClass(MetaClass metaclass)
        {
            var metaclassName = metaclass.Name;

            var stepDetails = $"List of {metaclass.GetPluralName(true)}:<br>";

            //Get all item for metaClass
            var metaClassItems = (await TimedStep($"Get all {metaclass.GetPluralName(true)}", GetAll, metaclassName,
                                        new List<Field>()
                                        {
                                            new ScalarField(MetaFieldNames.id, "String", false),
                                            new ScalarField(MetaFieldNames.name, "String", false),
                                        })).ToObject<List<JObject>>();

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
                await TimedStep($"Delete all {metaclass.GetPluralName(true)}", DeleteMulti, metaclassName, args);
            }

            DetailedStep(stepDetails);
            CountedStep($"Number of {metaclass.GetPluralName(true)} deleted", metaClassItems.Count);
        }

        protected void CompareListItems(MetaClass metaclass, List<JObject> originals, List<JObject> createdDatas)
        {
            //Convert created datas to dictionary to get each of them from their original id
            Dictionary<string, JObject> createdDatasById = new Dictionary<string, JObject>();
            createdDatas.ForEach(createdData => createdDatasById.Add(createdData [MetaFieldNames.externalIdentifier].ToString(), createdData));

            void CompareAll()
            {
                bool ok = true;
                int compareCount = 0;
                foreach(var original in originals)
                {
                    var originalId = original.GetValue(MetaFieldNames.id).ToString();
                    var createdData = createdDatasById [originalId];
                    bool compare = CompareItems(original, createdData, metaclass.OutputFieldsWrittable());
                    ok = ok && compare;
                    compareCount += compare ? 1 : 0;
                }
                CountedStep($"Number of {metaclass.GetPluralName(true)} copied successfully", compareCount, originals.Count);
            }
            TimedStep($"Compare all {metaclass.GetPluralName(true)} between repositories", CompareAll);
        }

        protected bool CompareItems(JObject original, JObject created, List<Field> fields)
        {
            bool ok = true;
            int compareCount = 0;
            int compareCountTotal = 0;
            var messageDetails = $"Object {original.GetValue(MetaFieldNames.id)}({original.GetValue(MetaFieldNames.name)}):<br>";

            fields.ForEach(field =>
            {
                if(field.Name == MetaFieldNames.externalIdentifier)
                {
                    return;
                }
                var value1 = original.GetValue(field.Name);
                var value2 = created.GetValue(field.Name);
                bool compare;
                if (field.Name == "name")
                {
                    var name1 = value1.Value<string>().Trim();
                    var name2 = value2.Value<string>().Trim();
                    if(name2.Length > name1.Length) // on vérifie si c'est une forme incrémentale: "nom-12345"
                    {
                        var suffix = name2.Substring(name1.Length);
                        var regex = new Regex("^-[0-9]*$");
                        if(regex.IsMatch(suffix))
                        {
                            name2 = name2.Substring(0, name1.Length);
                        }
                    }
                    compare = name1 == name2;
                }
                else
                {
                    compare = JToken.EqualityComparer.Equals(value1, value2);
                }
                ok = ok && compare;
                if(!compare)
                {
                    var messageValues = $"{field.Name}: {value1.ToString()} <===> {value2.ToString()}";
                    messageDetails += $"{ColoredMessageFromBool(messageValues, compare)}<br>";
                }
                ok = ok && compare;
                ++compareCountTotal;
                compareCount += compare ? 1 : 0;
            });
            if(!ok)
            {
                DetailedStep(messageDetails);
            }
            return ok;
        }

        protected void CompareLinksList(List<JObject> originals, List<JObject> copies, string linkName)
        {
            int linksCopied = 0;
            int linksExpected = 0;
            var copiesByExtIds = GetDatasByExternalId(copies);
            foreach(var original in originals)
            {
                var id = original.GetValue(MetaFieldNames.id).ToString();
                if(copiesByExtIds.TryGetValue(id, out var copy))
                {
                    var originalLinks = original.GetValue(linkName) as JArray;
                    var copyLinks = copy.GetValue(linkName) as JArray;
                    linksCopied += CompareLinks(originalLinks, copyLinks);
                    linksExpected += originalLinks.Count;
                }
            }
            CountedStep($"Number of {linkName} copied successfully", linksCopied, linksExpected);
        }

        private int CompareLinks(JArray originalLinks, JArray copyLinks)
        {
            SortedSet<string> GenerateSortedIds(JArray links, string fieldName)
            {
                var sortedIds = new SortedSet<string>();
                foreach(JObject link in links)
                {
                    var id = link.GetValue(fieldName).ToString();
                    sortedIds.Add(id);
                }
                return sortedIds;
            }
            var sortedOriginalIds = GenerateSortedIds(originalLinks, MetaFieldNames.id);
            var sortedCopyIds = GenerateSortedIds(copyLinks, MetaFieldNames.externalIdentifier);
            int count = 0;
            foreach(var originalId in sortedOriginalIds)
            {
                count += sortedCopyIds.Contains(originalId) ? 1 : 0;
            }
            return count;
        }

        protected string ColoredMessageFromBool(string message, bool ok)
        {
            return $"<span style=\"color: {(ok ? "green" : "red")}\">{message}</span>";
        }

        protected Dictionary<string, JObject> GetDatasByExternalId(List<JObject> datas)
        {
            var result = new Dictionary<string, JObject>();
            foreach (var data in datas)
            {
                var extId = data[MetaFieldNames.externalIdentifier].ToString();
                if (!string.IsNullOrEmpty(extId))
                {
                    result.Add(data[MetaFieldNames.externalIdentifier].ToString(), data);
                }
            }
            return result;
        }

        protected Dictionary<string, string> GetDatasIdByExternalId(List<JObject> datas)
        {
            var result = new Dictionary<string, string>();
            foreach (var data in datas)
            {
                if (data.ContainsKey(MetaFieldNames.externalIdentifier) && data.ContainsKey(MetaFieldNames.id))
                {
                    result.Add(data[MetaFieldNames.externalIdentifier].ToString(), data[MetaFieldNames.id].ToString());
                }
            }
            return result;
        }

        protected async Task TransferTest(MetaClass metaclass)
        {
            //Generate token
            //CreateAndSetToken();

            //Initialisation of metaclasses
            await TimedStep($"Get schema for class {metaclass.Name}", MetaclassSchema, metaclass);

            //Transfer part
            var (originals, created) = await TimedStep($"Copy all {metaclass.GetSingleName(true)}", CopyAll, metaclass);

            //Comparison part
            CompareListItems(metaclass, originals, created);
        }

        protected async Task LinkTransferTest(MetaClass metaclass1, MetaClass metaclass2, string linkName)
        {
            //Set source repository
            SetConfig(Source);

            //Get original datas for class1
            var outputFields = new List<Field>
            {
                new ScalarField(MetaFieldNames.id, "String", false),
                new ListField(linkName,
                    new ObjectField(null, new List<Field>
                    {
                        new ScalarField(MetaFieldNames.id, "String", false)
                    }, false), false)
            };
            var originals1 = (await TimedStep($"Get all {metaclass1.GetSingleName(true)} from source repository", GetAll, metaclass1.Name, outputFields)).ToObject<List<JObject>>();

            //Set source repository
            SetConfig(Destination);

            //Get created datas for class1
            outputFields = new List<Field>
            {
                new ScalarField(MetaFieldNames.id, "String", false),
                new ScalarField(MetaFieldNames.externalIdentifier, "String", false)
            };
            var created1 = (await TimedStep($"Get all {metaclass1.GetSingleName(true)} from target repository", GetAll, metaclass1.Name, outputFields)).ToObject<List<JObject>>();
            var createdByExtId1 = GetDatasByExternalId(created1);

            //Get created datas for class2
            outputFields.Add(new ScalarField(MetaFieldNames.id, "String", false));
            var created2 = (await TimedStep($"Get all {metaclass2.GetSingleName(true)} from target repository", GetAll, metaclass2.Name, outputFields)).ToObject<List<JObject>>();
            var createdIdByExtId2 = GetDatasIdByExternalId(created2);

            //Links
            await TimedStep($"Create links between {metaclass1.GetPluralName(true)} and {metaclass2.GetPluralName(true)}", Link, originals1, createdByExtId1, createdIdByExtId2, metaclass1, metaclass2, linkName);

            //Get links from target repository by external identifiers
            outputFields = new List<Field>
            {
                new ScalarField(MetaFieldNames.externalIdentifier, "String", false),
                new ListField(linkName,
                    new ObjectField(null, new List<Field>
                    {
                        new ScalarField(MetaFieldNames.externalIdentifier, "String", false)
                    }, false), false)
            };
            created1 = (await TimedStep($"Get all {linkName} from {metaclass1.GetSingleName(true)} from target repository", GetAll, metaclass1.Name, outputFields)).ToObject<List<JObject>>();

            //Compare links
            CompareLinksList(originals1, created1, linkName);
        }

        protected async Task DeletionTest(MetaClass metaclass)
        {
            SetConfig(Destination);

            //Delete all datas
            await TimedStep($"Deletion of {metaclass.GetPluralName(true)}", DeleteAllFromMetaClass, metaclass);

            //Get datas from database
            var outputFields = new List<Field>
            {
                new ScalarField(MetaFieldNames.id, "String"),
                new ScalarField(MetaFieldNames.name, "String")
            };
            var datas = await TimedStep($"Get all {metaclass.GetPluralName(true)}", GetAll, metaclass.Name, outputFields);

            //Ensure database is empty for this metaclass
            CountedStep($"Number of remaining {metaclass.GetPluralName(true)}", datas.Count, 0);
            if(datas.Count > 0)
            {
                var details = $"Remaining {metaclass.GetPluralName(true)}:<br>";
                foreach(JObject data in datas)
                {
                    details += $"- {data.GetValue(MetaFieldNames.name).ToString()} ({data.GetValue(MetaFieldNames.id).ToString()})<br>";
                }
                DetailedStep(details);
            }
        }

        protected async Task LinkDeletionTest(MetaClass metaclass, string linkName)
        {
            //Set token and config
            //CreateAndSetToken();
            SetConfig(Destination);

            //Delete links
            await TimedStep($"Deletion of {linkName} from {metaclass.GetPluralName(true)}", DeleteLinks, metaclass, linkName);

            //Ensure links are deleted correctly
            await CheckLinksDeleted(metaclass, linkName);
        }

        protected async Task CheckLinksDeleted(MetaClass metaclass, string linkName)
        {
            //Get links
            var outputFields = new List<Field>
            {
                new ScalarField(MetaFieldNames.id, "String"),
                new ScalarField(MetaFieldNames.name, "String"),
                new ListField(linkName,
                    new ObjectField(null, new List<Field>
                    {
                        new ScalarField(MetaFieldNames.id, "String"),
                        new ScalarField(MetaFieldNames.name, "String")
                    }))
            };
            var datas = await TimedStep($"Get all {linkName} from each {metaclass.GetSingleName(true)}", GetAll, metaclass.Name, outputFields);

            //Ensure links are removed
            var linksRemaining = new List<string>();
            foreach(JObject data in datas)
            {
                var dataName = data.GetValue(MetaFieldNames.name).ToString();
                var dataId = data.GetValue(MetaFieldNames.id).ToString();
                var details = $"{FormatElementIdName(dataId, dataName)} is attached to:<br>";
                var links = data.GetValue(linkName) as JArray;
                foreach(JObject link in links)
                {
                    var linkedName = link.GetValue(MetaFieldNames.name).ToString();
                    var linkedId = link.GetValue(MetaFieldNames.id).ToString();
                    linksRemaining.Add($"- {FormatElementIdName(linkedId, linkedName)}<br>");
                }
            }

            //Ensure database is empty for this metaclass
            CountedStep($"Number of remaining {linkName} from {metaclass.GetPluralName(true)}", linksRemaining.Count, 0);
            if(linksRemaining.Count > 0)
            {
                var details = $"Remaining {linkName} from {metaclass.GetPluralName(true)}:<br>";
                foreach(var link in linksRemaining)
                {
                    details += link;
                }
                DetailedStep(details);
            }
        }
    }
}
