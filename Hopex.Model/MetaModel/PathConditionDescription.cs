using Hopex.Model.Abstractions.MetaModel;
using Hopex.Model.PivotSchema.Models;

namespace Hopex.Model.MetaModel
{
    public class PathConditionDescription : IPathConditionDescription
    {
        public PathConditionDescription(PivotPathConditionDescription pathConditionDescription)
        {
            Id = Utils.NormalizeHopexId(pathConditionDescription.Id);
            RoleName = pathConditionDescription.RoleName;
            RoleId = Utils.NormalizeHopexId( pathConditionDescription.RoleId);
            MetaClassName = pathConditionDescription.MetaClassName;
            MetaClassId = Utils.NormalizeHopexId(pathConditionDescription.MetaClassId);
            Multiplicity = pathConditionDescription.Multiplicity;
            ObjectFilterId = pathConditionDescription.ObjectFilterId;
            ObjectFilterShortName = pathConditionDescription.ObjectFilterShortName;
        }

        public string Id { get; }
        public string RoleName { get; }
        public string RoleId { get; }
        public string MetaClassName { get; }
        public string MetaClassId { get; }
        public string Multiplicity { get; }
        public string ObjectFilterId { get; }
        public string ObjectFilterShortName { get; }
    }
}
