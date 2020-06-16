using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Primitives;
using System.Text;

namespace Mega.WebService.GraphQL.V3.UnitTests.Assertions
{
    static class ByteArrayExtension
    {
        public static ByteArrayAssertions Should(this byte[] instance)
        {
            return new ByteArrayAssertions(instance);
        }
    }

    class ByteArrayAssertions : ReferenceTypeAssertions<byte[], ByteArrayAssertions>
    {
        public ByteArrayAssertions(byte[] instance)
        {
            Subject = instance;
        }

        protected override string Identifier => "byte[]";

        public AndConstraint<ByteArrayAssertions> Be(string expected)
        {
            var actual = Encoding.ASCII.GetString(Subject)
                 .TrimEnd('\0');
            actual.Should().Be(expected);
            return new AndConstraint<ByteArrayAssertions>(this);
        }

        public AndConstraint<ByteArrayAssertions> BeEquivalentTo(byte[] expectation, string because = "", params object[] becauseArgs)
        {
            new GenericCollectionAssertions<byte>(Subject).BeEquivalentTo(expectation, because, becauseArgs);
            return new AndConstraint<ByteArrayAssertions>(this);
        }
    }
}
