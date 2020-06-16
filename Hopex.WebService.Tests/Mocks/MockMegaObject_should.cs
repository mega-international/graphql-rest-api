using FluentAssertions;
using Xunit;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaObject_should
    {
        [Fact]
        public void Compute_an_unnamed_field()
        {
            new MockMegaObject("759B777F4FF11543").MegaUnnamedField.Should().Be("~UkPT)TNyFDK5[X]");
        }

        [Fact]
        public void Keep_given_named_field_id()
        {
            new MockMegaObject("~UkPT)TNyFDK5[A given name]").MegaField.Should().Be("~UkPT)TNyFDK5[A given name]");
        }

        [Fact]
        public void Generate_a_named_field_for_an_object_without_and_id()
        {
            new MockMegaObject().MegaField.Should().Be("~000000000000[<Unknown Mock Object>]");
        }

        [Fact]
        public void Generate_a_named_field_for_an_empty_object()
        {
            new InexistingMockMegaObject().MegaField.Should().Be("~000000000000[<Empty Object>]");
        }

        [Fact]
        public void Replay_with_id64_on_GetProp()
        {
            new MockMegaObject("759B777F4FF11543").GetPropertyValue("~310000000D00").Should().Be("UkPT)TNyFDK5");
        }

    }
}
