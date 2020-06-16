using FluentAssertions;
using Mega.Macro.API;
using Xunit;

namespace Hopex.WebService.Tests.Mocks
{
    public class MegaIdConverter_should
    {
        [Theory]
        [InlineData("UkPT)TNyFDK5")]
        [InlineData("759B777F4FF11543")]
        [InlineData("~UkPT)TNyFDK5")]
        [InlineData("~UkPT)TNyFDK5[A name]")]
        [InlineData(3.0076444089972787E-206)]
        public void Convert_to_base_64(object id)
        {
            var megaId = MegaId.Create(id);
            MegaIdConverter.To64(megaId).Should().Be("UkPT)TNyFDK5");
        }

        [Theory]
        [InlineData("UkPT)TNyFDK5")]
        [InlineData("759B777F4FF11543")]
        [InlineData("~UkPT)TNyFDK5")]
        [InlineData("~UkPT)TNyFDK5[A name]")]
        [InlineData(3.0076444089972787E-206)]
        public void Convert_to_unnamed_field(object id)
        {
            var megaId = MegaId.Create(id);
            MegaIdConverter.ToUnnamedField(megaId).Should().Be("~UkPT)TNyFDK5[X]");
        }

        [Theory]
        [InlineData("~UkPT)TNyFDK5")]
        [InlineData("~UkPT)TNyFDK5[A name]")]
        public void Keep_named_field(string id)
        {
            var megaId = MegaId.Create(id);
            MegaIdConverter.ToField(megaId).Should().Be(id);
        }

        [Theory]
        [InlineData("UkPT)TNyFDK5")]
        [InlineData("759B777F4FF11543")]
        [InlineData(3.0076444089972787E-206)]
        public void Convert_to_named_field(object id)
        {
            var megaId = MegaId.Create(id);
            MegaIdConverter.ToField(megaId).Should().Be("~UkPT)TNyFDK5[Default mock name]");
        }
    }
}
