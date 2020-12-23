using Mega.WebService.GraphQL.Tests.Sources.FieldModels;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public static class MetaClassNames
    {
        //ITPM
        public const string Application = "Application";
        public const string BusinessProcess = "BusinessProcess";
        public const string BusinessCapability = "BusinessCapability";
        public const string SoftwareTechnology = "SoftwareTechnology";

        //Audit
        public const string Audit = "Audit";
        public const string AuditTheme = "AuditTheme";
        public const string AuditActivity = "AuditActivity";
        public const string WorkPaper = "WorkPaper";
        public const string TestSheet = "TestSheet";
        public const string QuestionAudit = "QuestionAudit";
        public const string AnswerAudit = "AnswerAudit";
        public const string Finding = "Finding";
        public const string Recommendation = "Recommendation";
    }

    public static class MetaFieldNames
    {
        public const string id = "id";
        public const string name = "name";
        public const string externalIdentifier = "externalId";
        public const string filter = "filter";
        public const string hexaIdAbs = "hexaIdAbs";
    }
    public abstract class MetaClass
    {
        public string Name => GetType().Name;
        public List<Field> InputFields, Fields;

        public virtual List<string> GetBlackListedFields()
        {
            return new List<string> { "customField",
                                    "linkComment",
                                    "linkCreatorId",
                                    "linkModifierId",
                                    "linkCreatorName",
                                    "linkModifierName",
                                    "linkCreationDate",
                                    "linkModificationDate",
                                    "order"};
        }

        public List<Field> OutputFieldsWrittable()
        {
            var result = new List<Field>();
            foreach(var field in Fields)
            {
                if(InputFields.Exists(input => input.Name == field.Name))
                {
                    result.Add(field);
                }
            }
            return result;
        }

        public string GetSingleName(bool lowerCase = false)
        {
            string name = GetSingleNameStartingWithUpperCase();
            return lowerCase ? name.ToLower() : name;
        }

        public string GetPluralName(bool lowerCase = false)
        {
            string name = GetPluralNameStartingWithUpperCase();
            return lowerCase ? name.ToLower() : name;
        }

        protected abstract string GetSingleNameStartingWithUpperCase();

        protected abstract string GetPluralNameStartingWithUpperCase();

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
