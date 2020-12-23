using System.Collections.Generic;

namespace Mega.WebService.GraphQL.Tests.Sources.Metaclasses
{
    public class Recommendation : MetaClass
    {
        public override List<string> GetBlackListedFields()
        {
            var fields = base.GetBlackListedFields();
            fields.AddRange(new List<string> {
                "recommendationStatus"});
            return fields;
        }
        protected override string GetSingleNameStartingWithUpperCase()
        {
            return "Recommendation";
        }

        protected override string GetPluralNameStartingWithUpperCase()
        {
            return "Recommendations";
        }
    }
}
