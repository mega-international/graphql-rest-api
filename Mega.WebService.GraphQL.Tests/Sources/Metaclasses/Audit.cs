using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class Audit : MetaClass
    {
        public override List<string> GetBlackListedFields()
        {
            var fields = base.GetBlackListedFields();
            fields.AddRange(new List<string> {
                "missionStatus"});
            return fields;
        }

        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Audit";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Audits";
        }

        public static class MetaFieldNames
        {
            public const string auditTheme = "auditTheme";
            public const string auditActivity = "auditActivity";
        }
    }
}
