namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class AnswerAudit : MetaClass
    {
        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Answer/audit";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Answers/audit";
        }
    }
}
