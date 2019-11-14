using GraphQL.Types;

namespace Hopex.Modules.GraphQL.Schema.Directives
{
    public class FormatDirective : DirectiveGraphType
    {
        public static FormatDirective Instance { get; } = new FormatDirective();

        private FormatDirective()
            : base("format", new[]
            {
                DirectiveLocation.Field,
                DirectiveLocation.FragmentSpread,
                DirectiveLocation.InlineFragment
            })
        {
            Description = "Define hopex format to use when getting property.";
            Arguments = new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>>
            {
                Name = "name",
                Description = "Format to use."
            });
        }
    }
}
