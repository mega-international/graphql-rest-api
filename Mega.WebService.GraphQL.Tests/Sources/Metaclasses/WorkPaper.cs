namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class WorkPaper : MetaClass
    {
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
