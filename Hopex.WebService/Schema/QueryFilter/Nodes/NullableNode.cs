namespace Hopex.Modules.GraphQL.Schema.QueryFilter.Nodes
{
    class NullableNode : Node<bool>
    {
        private bool IsNull => Value;
        public NullableNode(object isNull) : base(isNull) { }

        public override void Build() {}

        public override string GetQuery()
        {
            return IsNull ? "Null" : "Not Null";
        }
    }
}
