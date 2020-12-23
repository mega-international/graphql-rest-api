using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class AuditActivity : MetaClass
    {
        public override List<string> GetBlackListedFields()
        {
            var fields = base.GetBlackListedFields();
            fields.AddRange(new List<string> {
                "activityStatus"});
            return fields;
        }
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
