using Mega.WebService.GraphQL.Tests.Sources.FieldModels;
using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    public class Test19 : AbstractTest
    {
        protected enum ResultCode
        {
            Success,
            Timeout,
            Failure,
            Unknown
        }

        protected class TaskResult
        {
            public readonly string ActionName;
            public readonly ResultCode ResultCode;
            public readonly string Message;

            public TaskResult(string actionName, ResultCode resultCode)
            {
                ActionName = actionName;
                ResultCode = resultCode;
                switch (ResultCode)
                {
                    case ResultCode.Failure:
                        Message = $"Action {ActionName} failed";
                        break;

                    case ResultCode.Success:
                        Message = $"Action {ActionName} succeeded";
                        break;

                    case ResultCode.Timeout:
                        Message = $"Action {ActionName} timed out";
                        break;

                    case ResultCode.Unknown:
                        Message = $"Action {ActionName} unknown result";
                        break;
                }
            }

            public bool IsSuccess()
            {
                return ResultCode == ResultCode.Success;
            }
        }

        protected static class ActionNames
        {
            public const string GetActivity = "GetActivity";
            public const string CreateFindingInActivity = "CreateFindingInActivity";
            public const string GetFindingsInActivity = "GetFindingsInActivity";
            public const string UpdateActivity = "UpdateActivity";
            public const string DeleteFinding = "DeleteFinding";
            public const string GetFinding = "GetFinding";
        }

        protected override void Initialisation()
        {
            _requester = new GraphQLRequester($"{_myServiceUrl}/api/{(IsAsyncMode ? "async/" : "")}{_schemaAudit}");
            _requester2 = new GraphQLRequester($"{_myServiceUrl}/api/{(IsAsyncMode ? "async/" : "")}{_schemaAudit}")
            {
                Login = "scr",
                Password = "Hopex"
            };
        }

        private GraphQLRequester _requester2;

        private readonly string[] _actions = new string[]
        {
            ActionNames.CreateFindingInActivity,
            ActionNames.GetFindingsInActivity
        };

        public Test19(Parameters parameters) : base(parameters) { }

        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            SetConfig(_requester, EnvironmentId, RepositoryIdTo, ProfileId);
            SetConfig(_requester2, EnvironmentId, RepositoryIdTo, ProfileId);
            var activities = await GetAllIds();
            if (activities.Count <= 0)
            {
                throw new Exception("Missing data in audit activities");
            }
            await ProcessActionsAsync(activities, 10).ConfigureAwait(false);
        }

        private async Task<List<string>> GetIdsFromMetaclass(string metaclassName)
        {
            var outputs = new List<Field> { new ScalarField(MetaFieldNames.id, "String") };
            var oDatas = (await GetAll(metaclassName, outputs)).ToObject<List<JObject>>();
            var datas = new List<string>();
            foreach (var oData in oDatas)
            {
                var id = oData.GetValue(MetaFieldNames.id).ToString();
                datas.Add(id);
            }
            CountedStep($"Number of items from {metaclassName}", datas.Count);
            return datas;
        }

        private async Task<List<string>> GetAllIds()
        {
            var activities = await GetIdsFromMetaclass("AuditActivity");
            return activities;
        }

        private string GenerateRandom(IReadOnlyList<string> datas)
        {
            if (datas.Count > 1) //never generate if last data
            {
                var random = new Random();
                var idx = random.Next(0, datas.Count);
                return datas[idx];
            }
            return null;
        }

        protected async Task ProcessActionsAsync(List<string> activities, int count)
        {
            var asyncTasks = new List<Task<TaskResult>>();
            while (count-- > 0)
            {
                var id = GenerateRandom(activities);
                foreach (var actionName in _actions)
                {
                    asyncTasks.Add(ProcessActionAsync(_requester, actionName, id));
                    asyncTasks.Add(ProcessActionAsync(_requester2, actionName, id));
                    Thread.Sleep(500);
                }
            }
            var results = new TaskResult[asyncTasks.Count];
            var current = 0;
            var successCounter = 0;
            while (asyncTasks.Count > 0)
            {
                var finished = await Task.WhenAny(asyncTasks).ConfigureAwait(false);
                var result = finished.Result;
                DetailedStep(result.Message);
                if (result.IsSuccess())
                {
                    ++successCounter;
                }
                results[current++] = result;
                asyncTasks.Remove(finished);
            }
            CountedStep("Number of succeeded tasks", successCounter, results.Length);
        }

        protected async Task<TaskResult> ProcessActionAsync(GraphQLRequester requester, string actionName, string id)
        {
            if (id == null)
            {
                return new TaskResult(actionName, ResultCode.Unknown);
            }
            var method = GetType().GetMethod(actionName, BindingFlags.NonPublic | BindingFlags.Instance);
            CancellationTokenSource cts = new CancellationTokenSource(60000000);
            try
            {
                await TimedStepAsync($"Execution of task: {actionName}", ProcessActionFromName, requester, actionName, id, cts.Token).ConfigureAwait(false);
                return new TaskResult(actionName, ResultCode.Success);
            }
            catch (OperationCanceledException)
            {
                return new TaskResult(actionName, ResultCode.Timeout);
            }
            catch (Exception)
            {
                return new TaskResult(actionName, ResultCode.Failure);
            }
        }

        protected async Task ProcessActionFromName(GraphQLRequester requester, string name, string id, CancellationToken token)
        {
            switch (name)
            {
                case ActionNames.CreateFindingInActivity:
                    await CreateFindingInActivity(requester, id, token);
                    break;

                case ActionNames.GetFindingsInActivity:
                    await GetFindingsInActivity(requester, id, token);
                    break;
            }
        }

        //Step 5: Within an activity
        protected async Task CreateFindingInActivity(GraphQLRequester requester, string activityId, CancellationToken token)
        {
            string query = "mutation\n" +
                            "{\n" +
                                "createFinding(finding:\n" +
                                "{\n" +
                                    "name: \"name new finding\"\n" +
                                    "findingImpact: High\n" +
                                    "findingType: Weakness\n" +
                                    "detailedDescription: \"a\"\n" +
                                    "auditActivity_Activity:\n" +
                                    "{\n" +
                                        "action: ADD\n" +
                                        $"list:[{{ id:\"{activityId}\"}}]\n" +
                                    "}\n" +
                                "})\n" +
                                "{\n" +
                                    "id\n" +
                                    "name\n" +
                                "}\n" +
                            "}";
            await ProcessRawQuery(requester, query, token);
        }

        protected async Task GetFindingsInActivity(GraphQLRequester requester, string activityId, CancellationToken token)
        {
            string query = "query {\n" +
                            $"auditActivity(filter:{{ id: \"{activityId}\" }}) {{\n" +
                                "id\n" +
                                "name\n" +
                                "finding_ActivityFinding {\n" +
                                    "id\n" +
                                    "name\n" +
                                "findingImpact\n" +
                                "detailedDescription\n" +
                                "}\n" +
                            "}\n" +
                        "}";
            await ProcessRawQuery(requester, query, token);
        }
    }
}
