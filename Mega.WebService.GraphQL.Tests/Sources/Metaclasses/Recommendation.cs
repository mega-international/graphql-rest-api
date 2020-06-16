namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class Recommendation : MetaClass
    {
        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Recommendation";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Recommendations";
        }
    }
}
