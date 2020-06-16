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
    public class Test18 : AbstractTest
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
                switch(ResultCode)
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
        }

        private readonly string[] _actions = new string[]
        {
            //ActionNames.GetActivity,
            ActionNames.CreateFindingInActivity,
            ActionNames.GetFindingsInActivity
            //ActionNames.UpdateActivity,
            //ActionNames.DeleteFinding,
            //ActionNames.GetFinding
        };

        public Test18(Parameters parameters) : base(parameters) { }

        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            SetConfig(EnvironmentId, RepositoryIdTo, ProfileId);
            var (activities, findings) = await GetAllIds();
            if(activities.Count <= 0)
            {
                throw new Exception("Missing data in audit activities");
            }
            if(findings.Count <= 0)
            {
                throw new Exception("Missing data in findings");
            }
            await ProcessActionsAsync(activities, findings, 25).ConfigureAwait(false);
        }

        private async Task<List<string>> GetIdsFromMetaclass(string metaclassName)
        {
            var outputs = new List<Field> { new ScalarField(MetaFieldNames.id, "String") };
            var oDatas = (await GetAll(metaclassName, outputs)).ToObject<List<JObject>>();
            var datas = new List<string>();
            foreach(var oData in oDatas)
            {
                var id = oData.GetValue(MetaFieldNames.id).ToString();
                datas.Add(id);
            }
            CountedStep($"Number of items from {metaclassName}", datas.Count);
            return datas;
        }

        private async Task<(List<string>, List<string>)> GetAllIds()
        {
            var activities = await GetIdsFromMetaclass("AuditActivity");
            var findings = await GetIdsFromMetaclass("Finding");
            return (activities, findings);
        }

        private string GenerateRandom(IReadOnlyList<string> datas)
        {
            if(datas.Count > 1) //never generate if last data
            {
                var random = new Random();
                var idx = random.Next(0, datas.Count);
                return datas[idx];
            }
            return null;
        }

        protected async Task ProcessActionsAsync(List<string> activities, List<string> findings, int count)
        {
            var asyncTasks = new List<Task<TaskResult>>();
            while(count-- > 0)
            {
                var id = GenerateRandom(activities);
                foreach(var actionName in _actions)
                {
                    /*string id = actionName.EndsWith("Activity") ?
                                GenerateRandom(activities) :
                                GenerateRandom(findings);*/
                    asyncTasks.Add(ProcessActionAsync(actionName, id));
                }
            }
            var results = new TaskResult [asyncTasks.Count];
            var current = 0;
            var successCounter = 0;
            while(asyncTasks.Count > 0)
            {
                var finished = await Task.WhenAny(asyncTasks).ConfigureAwait(false);
                var result = finished.Result;
                DetailedStep(result.Message);
                if(result.IsSuccess())
                {
                    ++successCounter;
                }
                results [current++] = result;
                asyncTasks.Remove(finished);
            }
            CountedStep("Number of succeeded tasks", successCounter, results.Length);
        }

        protected async Task<TaskResult> ProcessActionAsync(string actionName, string id)
        {
            if(id == null)
            {
                return new TaskResult(actionName, ResultCode.Unknown);
            }
            var method = GetType().GetMethod(actionName, BindingFlags.NonPublic | BindingFlags.Instance);
            CancellationTokenSource cts = new CancellationTokenSource(60000000);
            try
            {
                await TimedStepAsync($"Execution of task: {actionName}", ProcessActionFromName, actionName, id, cts.Token).ConfigureAwait(false);
                return new TaskResult(actionName, ResultCode.Success);
            }
            catch(OperationCanceledException)
            {
                return new TaskResult(actionName, ResultCode.Timeout);
            }
            catch(Exception)
            {
                return new TaskResult(actionName, ResultCode.Failure);
            }
        }

        protected async Task ProcessActionFromName(string name, string id, CancellationToken token)
        {
            switch(name)
            {
                case ActionNames.GetActivity:
                    await GetActivity(id, token);
                    break;

                case ActionNames.CreateFindingInActivity:
                    await CreateFindingInActivity(id, token);
                    break;

                case ActionNames.GetFindingsInActivity:
                    await GetFindingsInActivity(id, token);
                    break;

                case ActionNames.UpdateActivity:
                    await UpdateActivity(id, token);
                    break;

                case ActionNames.DeleteFinding:
                    await DeleteFinding(id, token);
                    break;

                case ActionNames.GetFinding:
                    await GetFinding(id, token);
                    break;
            }
        }

        //Step 4: Display activity infos
        protected async Task GetActivity(string id, CancellationToken token)
        {
            var query = "query {\n" +
                          $"auditActivity(filter:{{ id: \"{id}\"}}) {{\n" +
                            "id\n" +
                            "name\n" +
                            "effectiveWorkload: activityEffectiveWorkloadHours\n" +
                            " estimatedWorkloadHours\n" +
                            "beginDate\n" +
                            "endDate:activityEndDate\n" +
                            "controlObjective:comment\n" +
                            "activityStatus\n" +
                          "}\n" +
                        "}";
            await ProcessRawQuery(query, token);
        }

        //Step 5: Within an activity
        protected async Task CreateFindingInActivity(string activityId, CancellationToken token)
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
            var result = await ProcessRawQuery(query, token);
        }

        protected async Task GetFindingsInActivity(string activityId, CancellationToken token)
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
            await ProcessRawQuery(query, token);
        }

        protected async Task UpdateActivity(string id, CancellationToken token)
        {
            string query = "mutation \n" +
                            "{\n" +
                                $"updateAuditActivity(id: \"{id}\" auditActivity:\n" +
                                "{\n" +
                                    "beginDate: \"2019-10-11\"\n" +
                                    "activityEndDate: \"2019-11-12\"\n" +
                                    "estimatedWorkloadHours: 15\n" +
                                    "computedActivityEffectiveWorkloadHours: 20\n" +
                                "})\n" +
                                "{\n" +
                                    "id\n" +
                                    "name\n" +
                                    "estimatedWorkloadHours\n" +
                                    "activityEffectiveWorkloadHours\n" +
                                    "beginDate\n" +
                                    "activityEndDate\n" +
                                "}\n" +
                            "}";
            await ProcessRawQuery(query, token);
        }

        //Step 6: Within findings list
        protected async Task DeleteFinding(string id, CancellationToken token)
        {
            string query = "mutation\n" +
                        "{\n" +
                            $"deleteFinding(id: \"{id}\" cascade: false)\n" +
                            "{\n" +
                                "id\n" +
                            "}\n" +
                        "}";
            await TimedStep("Process action delete finding", ProcessRawQuery, query, token);
        }

        protected async Task GetFinding(string id, CancellationToken token)
        {
            string query = "query {\n" +
                            $"finding(id: \"{id}\") {{\n" +
                                "id\n" +
                                "name\n" +
                                "findingImpact\n" +
                                "detailedDescription\n" +
                            "}\n" +
                        "}";
            await ProcessRawQuery(query, token);
        }
    }
}
