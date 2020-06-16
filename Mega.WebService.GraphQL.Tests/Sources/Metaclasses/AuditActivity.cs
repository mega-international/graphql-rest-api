namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class AuditActivity : MetaClass
    {
        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Audit activity";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Audit activities";
        }

        public static class MetaFieldNames
        {
            public const string finding_ActivityFinding = "finding_ActivityFinding";
            public const string auditTheme_ActivityTheme = "auditTheme_ActivityTheme";
            public const string workPaper_ActivityWorkPaper = "workPaper_ActivityWorkPaper";
        }
    }
}
