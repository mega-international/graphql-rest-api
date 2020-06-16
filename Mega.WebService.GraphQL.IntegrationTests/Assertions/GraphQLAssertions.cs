using FluentAssertions;
using FluentAssertions.Primitives;
using GraphQL;

namespace Mega.WebService.GraphQL.IntegrationTests.Assertions
{
    class GraphQLResponseAssertions<T> : ReferenceTypeAssertions<GraphQLResponse<T>, GraphQLResponseAssertions<T>>
    {
        public GraphQLResponseAssertions(GraphQLResponse<T> instance)
        {
            Subject = instance;
        }

        protected override string Identifier => "HttpResponseAssertions";

        public AndConstraint<GraphQLResponseAssertions<T>> HaveNoError()
        {
            Subject.Errors.Should().BeNullOrEmpty();
            return new AndConstraint<GraphQLResponseAssertions<T>>(this);
        }
    }

    static class GraphQLResponseExtension
    {
        public static GraphQLResponseAssertions<T> Should<T>(this GraphQLResponse<T> instance)
        {
            return new GraphQLResponseAssertions<T>(instance);
        }
    }
}
