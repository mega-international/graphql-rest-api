using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    public class Test15 : AbstractTest
    {
        public Test15(Parameters parameters) : base(parameters) { }

        protected override void Initialisation()
        {
            _requester = GenerateRequester($"{_myServiceUrl}/api/{(IsAsyncMode ? "async/" : "")}/{_schemaAudit}");
        }

        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            //Initialisation of classes and hierarchie by link names
            TreeMetaClasses treeMetaClasses = BuildTreeMetaClasses();

            //Get schemas for all metaclass
            await InitSchemas(treeMetaClasses);

            //Copy all datas
            await CopyTree(treeMetaClasses);
        }

        protected TreeMetaClasses BuildTreeMetaClasses()
        {
            var classAudit = GetMetaClass(MetaClassNames.Audit);
            var classAuditTheme = GetMetaClass(MetaClassNames.AuditTheme);
            var classAuditActivity = GetMetaClass(MetaClassNames.AuditActivity);
            var classWorkPaper = GetMetaClass(MetaClassNames.WorkPaper);
            var classTestSheet = GetMetaClass(MetaClassNames.TestSheet);
            var classQuestionAudit = GetMetaClass(MetaClassNames.QuestionAudit);
            var classAnswerAudit = GetMetaClass(MetaClassNames.AnswerAudit);
            var classFinding = GetMetaClass(MetaClassNames.Finding);
            var classRecommendation = GetMetaClass(MetaClassNames.Recommendation);

            TreeMetaClasses tree = new TreeMetaClasses(classAudit, "");
            tree.AddChildToCurrent(classAuditTheme, Audit.MetaFieldNames.auditTheme);
            tree.AddChildToCurrent(classAuditActivity, Audit.MetaFieldNames.auditActivity, true);

            tree.AddChildToCurrent(classAuditTheme, AuditActivity.MetaFieldNames.auditTheme_ActivityTheme);
            tree.AddChildToCurrent(classFinding, AuditActivity.MetaFieldNames.finding_ActivityFinding, true);

            tree.AddChildToCurrent(classRecommendation, Finding.MetaFieldNames.recommendation);
            tree.MoveCurrentToParent();

            tree.AddChildToCurrent(classWorkPaper, AuditActivity.MetaFieldNames.workPaper_ActivityWorkPaper, true);

            tree.AddChildToCurrent(classTestSheet, WorkPaper.MetaFieldNames.testSheet_WorkPaperTestSheet, true);

            tree.AddChildToCurrent(classQuestionAudit, TestSheet.MetaFieldNames.questionAudit_TestQuestion, true);

            tree.AddChildToCurrent(classAnswerAudit, QuestionAudit.MetaFieldNames.answerAudit_Answer, true);

            return tree;
        }

        protected async Task InitSchemas(TreeMetaClasses treeMetaClasses)
        {
            foreach(var metaclass in treeMetaClasses.MetaClasses)
            {
                await TimedStep($"Get schema for class {metaclass.Name}", MetaclassSchema, metaclass);
            }
        }

        protected async Task CopyTree(TreeMetaClasses treeMetaClasses)
        {
            //Set to source repository
            SetConfig(Source);

            //Get datas from first repository
            var outputFields = treeMetaClasses.GenerateOutputsFields();
            var originalAudits = (await TimedStep("Get all audits and subdatas from first repository", GetAll, MetaClassNames.Audit, outputFields)).ToObject<List<JObject>>();

            //Remove all audit with empty activities
            FilterItems(ref originalAudits);

            //Set to destination repository
            SetConfig(Destination);

            //Create data to second repository
            var originalsByMetaClass = treeMetaClasses.GetItemsByMetaClass(originalAudits);
            var copiesByMetaClass = new Dictionary<MetaClass, List<JObject>>();
            foreach(var originalsByMetaClassPair in originalsByMetaClass)
            {
                var metaclass = originalsByMetaClassPair.Key;
                var originals = originalsByMetaClassPair.Value;
                SetExternalIds(ref originals);
                var copies = await TimedStep($"Create all {metaclass.GetPluralName(true)} to second repository", CreateMulti, metaclass.Name, originals, metaclass.InputFields, metaclass.Fields);
                ClearExternalIds(ref originals);
                copiesByMetaClass.Add(metaclass, copies);
                WriteMessagesCreatedItems(metaclass, copies);
            }

            //Add links
            var pathsByMetaClass = treeMetaClasses.GeneratePathsByMetaClass();
            foreach(var originalsByMetaClassPair in originalsByMetaClass)
            {
                var metaclass1 = originalsByMetaClassPair.Key;
                var originals1 = originalsByMetaClassPair.Value;
                var copies1 = copiesByMetaClass [metaclass1];
                var copiesByExtId1 = GetDatasByExternalId(copies1);
                var paths = pathsByMetaClass[metaclass1];
                foreach(var path in paths)
                {
                    var linkName = path.Item1;
                    var metaclass2 = path.Item2;
                    var copies2 = copiesByMetaClass [metaclass2];
                    var copiesIdByExtId2 = GetDatasIdByExternalId(copies2);
                    await Link(originals1, copiesByExtId1, copiesIdByExtId2, metaclass1, metaclass2, linkName);
                }
            }

            //Get copies
            var copiesAudits = (await TimedStep("Get all audits and subdatas from second repository", GetAll, MetaClassNames.Audit, outputFields)).ToObject<List<JObject>>();
            copiesByMetaClass = treeMetaClasses.GetItemsByMetaClass(copiesAudits);

            //Compare items
            foreach(var originalsByMetaClassPair in originalsByMetaClass)
            {
                var metaclass = originalsByMetaClassPair.Key;
                var originals = originalsByMetaClassPair.Value;
                var copies = copiesByMetaClass [metaclass];
                CompareListItems(metaclass, originals, copies);
            }

            //Compare links
            foreach(var originalsByMetaClassPair in originalsByMetaClass)
            {
                var metaclass = originalsByMetaClassPair.Key;
                var originals = originalsByMetaClassPair.Value;
                var copies = copiesByMetaClass [metaclass];
                var paths = pathsByMetaClass [metaclass];
                foreach(var path in paths)
                {
                    var linkName = path.Item1;
                    CompareLinksList(originals, copies, linkName);
                }
            }
        }

        private void FilterItems(ref List<JObject> items)
        {
            items.RemoveAll(audit =>
            {
                var activities = audit.GetValue(Audit.MetaFieldNames.auditActivity) as JArray;
                return activities.Count <= 0;
            });
        }
    }
}
