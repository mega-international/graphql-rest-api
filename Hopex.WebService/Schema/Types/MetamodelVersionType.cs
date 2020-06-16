using GraphQL.Types;
using Mega.Macro.API;
using Mega.Macro.API.Library;

namespace Hopex.Modules.GraphQL.Schema.Types
{
    public class MetamodelVersionType : ObjectGraphType
    {
        public MetamodelVersionType()
        {
            Field<StringGraphType>("id", resolve: context =>
            {
                var currentEnvironmentVersion = context.Source as MegaObject;
                return currentEnvironmentVersion?.GetPropertyValue(MetaAttributeLibrary.AbsoluteIdentifier);
            });
            Field<StringGraphType>("name", resolve: context =>
            {
                var currentEnvironmentVersion = context.Source as MegaObject;
                return currentEnvironmentVersion?.GetPropertyValue(MetaAttributeLibrary.Name);
            });
        }
    }
}
