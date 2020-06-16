using FluentAssertions;
using FluentAssertions.Primitives;
using FluentAssertions.Json;
using Hopex.ApplicationServer.WebServices;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Hopex.Common.JsonMessages;
using System;

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
            var property = Subject.GetType().GetProperty("Value", BindingFlags.NonPublic | BindingFlags.Instance);
            var actual = (string)property.GetValue(Subject);
            Subject.StatusCode.Should().Be(200, $"no error should be in {actual}");
            Subject.ContentType.Should().Be("application/json", $"JSON expected in {actual}");            
            return JToken.Parse(actual);
        }

        public AndConstraint<HopexResponseAssertions> HaveNoGraphQLError()
        {
            var response = ParseGraphQLResponse();
            var selectedToken = response.SelectToken("errors");
            selectedToken.Should().BeNull($"no errors expected in {selectedToken}");
            return new AndConstraint<HopexResponseAssertions>(this);
        }

        public AndConstraint<HopexResponseAssertions> ContainsGraphQLCount(string path, int expected)
        {
            var jArray = SelectJsonArray(path);
            jArray.Count.Should().Be(expected);
            return new AndConstraint<HopexResponseAssertions>(this);
        }

        public AndConstraint<HopexResponseAssertions> ContainsGraphQLCountGreaterThan(string path, int expected)
        {
            var jArray = SelectJsonArray(path);
            jArray.Count.Should().BeGreaterThan(expected);
            return new AndConstraint<HopexResponseAssertions>(this);
        }

        public AndConstraint<HopexResponseAssertions> MatchAllGraphQL(string path, string childPath, string wildcardPattern)
        {
            var jArray = SelectJsonArray(path);
            foreach (var child in jArray.Children())
            {
                var childToken = child.SelectToken(childPath);
                childToken.Should().NotBeNull($"data expected for path {childPath} in JSON {childToken}");
                childToken.ToString().Should().Match(wildcardPattern, $"all elements of {jArray} should match \"{wildcardPattern}\" for path {childPath}");
            }
            return new AndConstraint<HopexResponseAssertions>(this);
        }

        public AndConstraint<HopexResponseAssertions> ContainsGraphQL(string path, string expected)
        {
            ParseGraphQLResponse().SelectToken(path).ToString().Should().Be(expected);
            return new AndConstraint<HopexResponseAssertions>(this);
        }

        public AndConstraint<HopexResponseAssertions> MatchGraphQL(string path, string wildcardPattern)
        {
            var response = ParseGraphQLResponse();
            var selectedToken = response.SelectToken(path);
            selectedToken.Should().NotBeNull($"data expected for path {path} in JSON {response}");
            selectedToken.ToString().Should().Match(wildcardPattern);
            return new AndConstraint<HopexResponseAssertions>(this);
        }

        private JObject ParseGraphQLResponse()
        {
            var serializedGraphQLResponse = (JObject)ParseJsonBody();            
            return JObject.Parse(serializedGraphQLResponse["Result"].ToString());
        }

        private JArray SelectJsonArray(string path)
        {
            var response = ParseGraphQLResponse();
            var selectedToken = response.SelectToken(path);
            selectedToken.Should().NotBeNull($"data expected for path {path} in JSON {response.ToString()}");
            var jArray = selectedToken.As<JArray>();
            jArray.Should().NotBeNull($"array expected for path {path} in JSON {response.ToString()}");
            return jArray;
        }

        public AndConstraint<HopexResponseAssertions> BeError(int statusCode)
        {
            var errorResponse = ParseJsonBody().ToObject<ErrorMacroResponse>();
            errorResponse.HttpStatusCode.Should().Be(statusCode);
            return new AndConstraint<HopexResponseAssertions>(this);
        }
    }
}
