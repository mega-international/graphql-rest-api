using GraphQL.Types;
using Hopex.Model.Abstractions.MetaModel;
using Hopex.Modules.GraphQL.Schema.Filters;

using static Hopex.Model.MetaModel.Constants;

namespace Hopex.Modules.GraphQL.Schema
{
    class GraphQLArgumentsFactory
    {
        private readonly FilterArgumentBuilder _filterArgumentBuilder;
        private readonly SchemaMappingResolver _schemaMappingResolver;        

        internal GraphQLArgumentsFactory(FilterArgumentBuilder builder, SchemaMappingResolver schemaMappingResolver)
        {
            _schemaMappingResolver = schemaMappingResolver;
            _filterArgumentBuilder = builder;
        }

        internal QueryArguments BuildRelationshipArguments(IRelationshipDescription link, HopexEnumerationGraphType languagesType)
        {
            var arguments = new QueryArguments();

            var addErqlFilterArguments = !(link.RoleId == MAEID_DESCRIBEDELEMENT_ABSTRACTDIAGRAM || link.RoleId == MAEID_METACLASS_SUBMETACLASS || link.RoleId == MAEID_METACLASS_SUPERMETACLASS);
            if (addErqlFilterArguments)
                _filterArgumentBuilder.AddFilterArguments(arguments, link.TargetClass, languagesType);

            var addSchemaFilterArgument = link.RoleId == MAEID_METACLASS_SUBMETACLASS || link.RoleId == MAEID_METACLASS_SUPERMETACLASS;
            if (addSchemaFilterArgument)
                _schemaMappingResolver.AddSchemaQueryArgument(arguments, link.RoleId);

            return arguments;
        }

    }
}
