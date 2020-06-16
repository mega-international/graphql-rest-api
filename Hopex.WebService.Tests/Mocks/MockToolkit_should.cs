using FluentAssertions;
using FluentAssertions.Json;
using Hopex.WebService.Tests.Assertions;
using Mega.Macro.API;
using Xunit;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockToolkit_should
    {
        readonly MockToolkit tk = new MockToolkit();

        [Fact]
        public void Equal_two_id_on_same_object_ref()
        {
            var id = MegaId.Create("aMRn)bUIGjX3");
            tk.IsSameId(id, id).Should().BeTrue();
        }

        [Fact]
        public void Equals_two_id_based_on_same_64_string()
        {
            tk.IsSameId(MegaId.Create("aMRn)bUIGjX3"), MegaId.Create("aMRn)bUIGjX3")).Should().BeTrue();
        }

        [Theory]
        [InlineData("aMRn)bUIGjX3", true)]
        [InlineData("UkPT)TNyFDK5", false)]
        public void Compare_a_64id_and_a_field(string id64, bool expected)
        {
            tk.IsSameId(id64, "~aMRn)bUIGjX3[System Business Document]").Should().Be(expected);
        }

        [Theory]
        [InlineData("C5B5E97F50490E1B", true)]
        [InlineData("759B777F4FF11543", false)]
        public void Compare_a_64id_with_an_hexaId(string idHexa, bool expected)
        {
            tk.IsSameId(idHexa, "aMRn)bUIGjX3").Should().Be(expected);
        }

        [Theory]
        [InlineData(3.0076444089972787E-206, true)]
        [InlineData(123d, false)]
        public void Compare_a_doubleid_with_an_hexaId(double idDouble, bool expected)
        {
            tk.IsSameId(MegaId.Create(idDouble), "759B777F4FF11543").Should().Be(expected);
        }

        [Fact]
        public void Compare_with_a_field_of_length_16()
        {
            tk.IsSameId("~B0SNPuLckCQ3[X]", "B0SNPuLckCQ3").Should().BeTrue();
        }
        
    }
}
