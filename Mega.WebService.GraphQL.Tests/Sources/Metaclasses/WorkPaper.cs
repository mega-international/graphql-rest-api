using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class WorkPaper : MetaClass
    {
        public override List<string> GetBlackListedFields()
        {
            var fields = base.GetBlackListedFields();
            fields.AddRange(new List<string> {
                "lastAssessmentDate"});
            return fields;
        }
        
        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Work paper";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Work papers";
        }

        public static class MetaFieldNames
        {
            public const string testSheet_WorkPaperTestSheet = "testSheet_WorkPaperTestSheet";
        }
    }
}
