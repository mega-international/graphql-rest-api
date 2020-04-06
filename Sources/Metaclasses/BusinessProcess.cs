namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class BusinessProcess : MetaClass
    {
        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Business process";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Business processes";
        }
    }
}
