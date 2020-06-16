namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class TestSheet : MetaClass
    {
        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Test sheet";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Test sheets";
        }

        public static class MetaFieldNames
        {
            public const string questionAudit_TestQuestion = "questionAudit_TestQuestion";
        }
    }
}
