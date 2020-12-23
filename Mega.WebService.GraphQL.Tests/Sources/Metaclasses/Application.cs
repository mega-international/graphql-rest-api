using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class Application : MetaClass
    {

        public override List<string> GetBlackListedFields()
        {
            var fields = base.GetBlackListedFields();
            fields.AddRange(new List<string> {
                "costperUser",
                "expenses",
                "capitalExpenses",
                "operatingExpenses",
                "globalExpense",
                "cASTHighlightID",
                "softwareResiliencyScoreCASTHighligh",
                "softwareAgilityScore",
                "softwareEleganceScoreCASTHighligh",
                "cloudReadyScoreCASTHighlight",
                "cloudReadyScanCASTHighlight",
                "technicalDebtCast",
                "numberOfLinesOfCodeCast",
                "numberOfFilesCast",
                "backFiredFunctionPointsCASTHighlight",
                "workflowStatusITAndBusinessData",
                "workflowStatusFinancial"});
            return fields;
        }

        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Application";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Applications";
        }

        public static class MetaFieldNames
        {
            public const string businessCapability = "businessCapability";
            public const string businessProcess = "businessProcess";
            public const string softwareTechnology_UsedTechnology = "softwareTechnology_UsedTechnology";
        }
    }
}
