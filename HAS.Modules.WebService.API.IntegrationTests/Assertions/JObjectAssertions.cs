using FluentAssertions;
using FluentAssertions.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace HAS.Modules.WebService.API.IntegrationTests.Assertions
{
    class JObjectAssertions : GenericDictionaryAssertions<string, JToken>
    {
        public JObjectAssertions(JObject instance) : base(instance) {}

        protected override string Identifier => "JSONObjectAssertions";

        public void MatchJSON(string jsonExpected)
        {
            var expected = JObject.Parse(jsonExpected);
            Subject.Should().BeEquivalentTo(expected);
        }
    }

    static class JObjectAssertionsExtension
    {
        public static JObjectAssertions Should(this JObject instance)
        {
            return new JObjectAssertions(instance);
        }
    }
}
