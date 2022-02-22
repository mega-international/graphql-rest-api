namespace Hopex.Modules.GraphQL.Schema.QueryFilter.Nodes
{
    internal class AttributeNameNode : Node
    {
        private readonly string _attributeName;
        public AttributeNameNode(string attributeName)
        {
            _attributeName = attributeName;
        }

        public override void Build() {}

        public override string GetQuery()
        {
            return _attributeName;
        }
    }
}
