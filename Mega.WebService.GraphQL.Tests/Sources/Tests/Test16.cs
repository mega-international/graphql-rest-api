using Mega.WebService.GraphQL.Tests.Sources.Metaclasses;
using System.Threading.Tasks;

namespace Mega.WebService.GraphQL.Tests.Sources.Tests
{
    public class Test16 : AbstractTest
    {
        public Test16(Parameters parameters) : base(parameters) { }

        protected override void Initialisation()
        {
            _requester = GenerateRequester($"{_myServiceUrl}/api/{(IsAsyncMode ? "async/" : "")}/{_schemaAudit}");
        }

        protected override async Task StepsAsync(ITestParam oTestParam)
        {
            //Initialisation of classes and hierarchie by link names
            TreeMetaClasses treeMetaClasses = BuildTreeMetaClasses();

            //Deletion of links
            var pathsByMetaClass = treeMetaClasses.GeneratePathsByMetaClass();
            foreach(var pathsByMetaClassPair in pathsByMetaClass)
            {
                var metaclass = pathsByMetaClassPair.Key;
                var paths = pathsByMetaClassPair.Value;
                foreach(var path in paths)
                {
                    var linkName = path.Item1;
                    await LinkDeletionTest(metaclass, linkName);
                }
            }

            //Deletion of datas
            var metaclasses = treeMetaClasses.MetaClasses;
            foreach(var metaclass in metaclasses)
            {
                await DeletionTest(metaclass);
            }
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
    }
}
