using Hopex.Model.Abstractions.MetaModel;

namespace Hopex.Model.MetaModel
{
    internal class GenericClassDescription : ClassDescription
    {
        public static string GenericName => "GraphQLObject";
        public override bool IsGeneric => true;
        public GenericClassDescription(IHopexMetaModel schema) : base(schema, GenericName, "", "", false) {}
    }
}
