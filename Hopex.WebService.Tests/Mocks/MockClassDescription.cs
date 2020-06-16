using Hopex.Model.Abstractions;
using Mega.Macro.API;
using System;

namespace Hopex.WebService.Tests.Mocks
{
    internal class MockClassDescription : MockMegaObject
    {
        MockMegaCollection _properties;

        public MockClassDescription(string id) : base(id)
        {
            _properties = new MockMegaCollection("~7fs9P58ig1fC[Properties]");
            WithRelation(new MockMegaCollection("~1fs9P5egg1fC[Description]")
                .WithChildren(new MockMegaObject()
                    .WithRelation(_properties)));
        }

        internal MockClassDescription WithMetaProperty(MockPropertyDescription mockMegaObject)
        {
            _properties.WithChildren(mockMegaObject);
            return this;
        }

        internal new MockClassDescription WithRelation(MockMegaCollection col)
        {
            return (MockClassDescription) base.WithRelation(col);
        }
    }
}
