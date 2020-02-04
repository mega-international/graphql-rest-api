namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class QuestionAudit : MetaClass
    {
        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Question/audit";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Questions/audit";
        }

        public static class MetaFieldNames
        {
            public const string answerAudit_Answer = "answerAudit_Answer";
        }
    }
}
