using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models.Metaclasses
{
    public class Application : MetaClass
    {
        public override List<string> GetBlackListedFields()
        {
            return new List<string> { "costperUser", "expenses", "capitalExpenses", "operatingExpenses", "globalExpense" };
        }

        public override string GetFieldNameFromLinkedMetaClass(string metaclassName)
        {
            switch(metaclassName)
            {
                case MetaClassNames.BusinessCapability:
                {
                    return "businessCapability";
                }
                case MetaClassNames.BusinessProcess:
                {
                    return "businessProcess";
                }
                case MetaClassNames.SoftwareTechnology:
                {
                     return "softwareTechnology_UsedTechnology";
                }
            }
            return null;
        }

        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Application";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Applications";
        }
    }
}
