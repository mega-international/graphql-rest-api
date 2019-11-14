using Mega.WebService.GraphQL.Tests.Models.FieldModels;
using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Models.Metaclasses
{
    public static class MetaClassNames
    {
        public const string Application = "Application";
        public const string BusinessProcess = "BusinessProcess";
        public const string BusinessCapability = "BusinessCapability";
        public const string SoftwareTechnology = "SoftwareTechnology";
    }

    public static class MetaFieldNames
    {
        public const string id = "id";
        public const string name = "name";
        public const string externalIdentifier = "externalIdentifier";
    }
    public abstract class MetaClass
    {
        public string Name => GetType().Name;
        public List<Field> InputFields, Fields;

        public virtual List<string> GetBlackListedFields()
        {
            return new List<string>();
        }

        public virtual string GetFieldNameFromLinkedMetaClass(string metaclassName)
        {
            return null;
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
    }
}
