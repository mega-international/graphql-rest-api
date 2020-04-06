using FluentAssertions;
using Hopex.Model.Abstractions.DataModel;
using Hopex.Model.DataModel;
using Hopex.Modules.GraphQL.Schema;
using Moq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Hopex.WebService.Tests
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SchemaBuilder_should
    {
        [Fact]
        public void GetFirstPaginatedModelElements()
        {
            var mockModelElement_1 = CreateMockModelElement("1");
            var mockModelElement_2 = CreateMockModelElement("2");
            var mockModelElement_3 = CreateMockModelElement("3");
            var mockModelElement_4 = CreateMockModelElement("4");
            var mockModelElement_5 = CreateMockModelElement("5");
            var orderedCollection = new List<IModelElement>
            {
                mockModelElement_1,
                mockModelElement_2,
                mockModelElement_3,
                mockModelElement_4,
                mockModelElement_5,
            };
            var arguments = new Dictionary<string, object>
            {
                {"first", 2},
                {"after", "1"},
                {"before", "4"},
                {"skip", 1}
            };
            var result = SchemaBuilder.GetPaginatedModelElements(orderedCollection, arguments);
            result.Should().Contain(mockModelElement_3);
        }

        [Fact]
        public void GetLastPaginatedModelElements()
        {
            var mockModelElement_1 = CreateMockModelElement("1");
            var mockModelElement_2 = CreateMockModelElement("2");
            var mockModelElement_3 = CreateMockModelElement("3");
            var mockModelElement_4 = CreateMockModelElement("4");
            var mockModelElement_5 = CreateMockModelElement("5");
            var orderedCollection = new List<IModelElement>
            {
                mockModelElement_1,
                mockModelElement_2,
                mockModelElement_3,
                mockModelElement_4,
                mockModelElement_5,
            };
            var arguments = new Dictionary<string, object>
            {
                {"last", 2},
                {"after", "1"},
                {"before", "4"},
                {"skip", 1}
            };
            var result = SchemaBuilder.GetPaginatedModelElements(orderedCollection, arguments);
            result.Should().Contain(mockModelElement_2);
        }

        private static IModelElement CreateMockModelElement(string id)
        {
            var mockModelElement = new Mock<IModelElement>();
            mockModelElement.Setup(x => x.Id).Returns(id);
            mockModelElement.Setup(x => x.GetCrud()).Returns(new CrudResult("CRUD"));
            mockModelElement.Setup(x => x.IsConfidential).Returns(false);
            mockModelElement.Setup(x => x.IsAvailable).Returns(true);
            return mockModelElement.Object;
        }
    }
}
