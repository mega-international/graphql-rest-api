namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class BusinessCapability : MetaClass
    {
        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Business capability";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Business capabilities";
        }
    }
}
