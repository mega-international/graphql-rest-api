namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class AuditTheme : MetaClass
    {
        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Audit theme";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Audit themes";
        }
    }
}
