using FluentAssertions;
using FluentAssertions.Primitives;
using FluentAssertions.Json;
using Hopex.ApplicationServer.WebServices;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System;
using Newtonsoft.Json;
using Hopex.Common.JsonMessages;

namespace Hopex.WebService.Tests.Assertions
{
    static class HopexResponseExtension
    {
        public static HopexResponseAssertions Should(this HopexResponse instance)
        {
            return new HopexResponseAssertions(instance);
        }
    }

    class HopexResponseAssertions : ReferenceTypeAssertions<HopexResponse, HopexResponseAssertions>
    {
        public HopexResponseAssertions(HopexResponse instance)
        {
            Subject = instance;
        }

        protected override string Identifier => "HopexResponse";

        public AndConstraint<HopexResponseAssertions> BeJson(string expected)
        {
            ParseJsonBody().Should().BeEquivalentTo(JToken.Parse(expected));
            return new AndConstraint<HopexResponseAssertions>(this);
        }

        public AndConstraint<HopexResponseAssertions> ContainsJson(string path, string expected)
        {
            ParseJsonBody().SelectToken(path).ToString().Should().Be(expected);
            return new AndConstraint<HopexResponseAssertions>(this);
        }

        public AndConstraint<HopexResponseAssertions> ContainsJsonCount(string path, int expected)
        {
            ParseJsonBody().SelectToken(path).As<JArray>().Count.Should().Be(expected);
            return new AndConstraint<HopexResponseAssertions>(this);
        }

        public AndConstraint<HopexResponseAssertions> MatchJson(string path, string wildcardPattern)
        {
            ParseJsonBody().SelectToken(path).ToString().Should().Match(wildcardPattern);
            return new AndConstraint<HopexResponseAssertions>(this);
        }      

        private JToken ParseJsonBody()
        {
            Subject.StatusCode.Should().Be(200);
            Subject.ContentType.Should().Be("application/json");
            var property = Subject.GetType().GetProperty("Value", BindingFlags.NonPublic | BindingFlags.Instance);
            var actual = (string)property.GetValue(Subject);
            return JToken.Parse(actual);
        }

        public AndConstraint<HopexResponseAssertions> ContainsGraphQLCount(string path, int expected)
        {
            ParseGraphQLResponse().SelectToken(path).As<JArray>().Count.Should().Be(expected);
            return new AndConstraint<HopexResponseAssertions>(this);
        }

        public AndConstraint<HopexResponseAssertions> ContainsGraphQL(string path, string expected)
        {
            ParseGraphQLResponse().SelectToken(path).ToString().Should().Be(expected);
            return new AndConstraint<HopexResponseAssertions>(this);
        }

        public AndConstraint<HopexResponseAssertions> MatchGraphQL(string path, string wildcardPattern)
        {
            ParseGraphQLResponse().SelectToken(path).ToString().Should().Match(wildcardPattern);
            return new AndConstraint<HopexResponseAssertions>(this);
        }

        private JObject ParseGraphQLResponse()
        {
            var serializedGraphQLResponse = (JObject)ParseJsonBody();            
            return JObject.Parse(serializedGraphQLResponse["Result"].ToString());
        }

        public AndConstraint<HopexResponseAssertions> BeError(int statusCode)
        {
            var errorResponse = ParseJsonBody().ToObject<ErrorMacroResponse>();
            errorResponse.HttpStatusCode.Should().Be(statusCode);
            return new AndConstraint<HopexResponseAssertions>(this);
        }
    }
}
