namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class Finding : MetaClass
    {
        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Finding";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Findings";
        }

        public static class MetaFieldNames
        {
            public const string recommendation = "recommendation";
        }
    }
}
