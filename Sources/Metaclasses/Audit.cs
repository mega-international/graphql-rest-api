namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class Audit : MetaClass
    {
        /*public override List<string> GetBlackListedFields()
        {
            return new List<string> { "costperUser", "expenses", "capitalExpenses", "operatingExpenses", "globalExpense" };
        }*/

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
