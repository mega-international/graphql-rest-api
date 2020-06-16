namespace Hopex.Modules.GraphQL.Schema.Filters
{
    struct HopexOperator
    {
        public string Name;
        public string Pattern;
        public bool IsNegation;

        public HopexOperator(string name, string pattern, bool isNegation=false) : this()
        {
            Name = name;
            Pattern = pattern;
            IsNegation = isNegation;
        }

        public static HopexOperator EqualOperator => new HopexOperator("=", null);
    }
}
