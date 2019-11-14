namespace Mega.WebService.GraphQL.Tests.Models.Metaclasses
{
    public class SoftwareTechnology : MetaClass
    {
        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Software technology";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Software technologies";
        }
    }
}
