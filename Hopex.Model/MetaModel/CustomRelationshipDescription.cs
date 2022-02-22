using Hopex.Model.Abstractions.DataModel;
using System.Collections.Generic;

namespace Hopex.Model.MetaModel
{
    internal class CustomRelationshipDescription : RelationshipDescription
    {
        public CustomRelationshipDescription(string relationId) : base(relationId, null, null, $"{relationId}[customRelationship]", relationId, "", null)
        {
            var linkDescritpion = new PathDescription(relationId, relationId, relationId, new ClassDescription(null, "unknow target", null, null, false), null, null);
            SetPath(new PathDescription [] { linkDescritpion });
        }

        public override IEnumerable<ISetter> CreateSetters(object value)
        {
            return CustomRelationshipSetter.CreateSetters(value);
        }
    }
}
