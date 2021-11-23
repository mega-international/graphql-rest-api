using FluentAssertions;
using FluentAssertions.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace HAS.Modules.WebService.API.IntegrationTests.Assertions
{
    class JArrayAssertions : GenericCollectionAssertions<JToken>
    {
        public JArrayAssertions(JArray instance) : base(instance)
        {
            Subject = instance;
        }

        protected JArray Instance => Subject as JArray;
        protected override string Identifier => "JSONArrayAssertions";

        public void MatchJSON(string jsonExpected)
        {
            var expected = JArray.Parse(jsonExpected);
            Subject.Should().BeEquivalentTo(expected);
        }
    }

    static class JArrayAssertionExtension
    {
        public static JArrayAssertions Should(this JArray instance)
        {
            return new JArrayAssertions(instance);
        }
    }
}
