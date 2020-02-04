using Hopex.Model.Mocks;
using Hopex.WebService.Tests.Mocks.Drawings;
using Mega.Macro.API;
using System;
using System.Collections.Generic;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaObject : MockMegaItem, IMegaObject
    {
        public MegaId Id { get; internal set; }

        public IMegaDrawing _drawing;

        private MegaId _classId;

        private Dictionary<MegaId, object> _properties = new Dictionary<MegaId, object>(new MegaIdComparer());
        private Dictionary<MegaId, MockMegaCollection> _links = new Dictionary<MegaId, MockMegaCollection>(new MegaIdComparer());

        public MockMegaObject(MegaId id, MegaId classId = null)
        {
            Id = id;
            _classId = classId ?? MegaId.Create("000000000000");
        }

        public MockMegaObject(string id)
            :this(MegaId.Create(id), null)
        {
        }

        public MockMegaObject WithDrawing(MockMegaDrawing drawing)
        {
            drawing._diagramObject = this;
            _drawing = drawing;
            return this;
        }

        public MockMegaObject()
        {
        }

        internal MockMegaObject WithRelation(MockMegaCollection col)
        {
            _links.Add(col.Id, col);
            return this;
        }

        public IMegaCollection GetCollection(MegaId linkId, int sortDirection1 = 1, string sortAttribute1 = null, int sortDirection2 = 1, string sortAttribute2 = null)
        {
            return _links[linkId];
        }

        public virtual MegaId GetClassId()
        {
            return _classId;
        }
        
        public string GetPropertyValue(MegaId propertyId, string format = "ASCII")
        {
            return (string) _properties[propertyId];
        }

        public void SetPropertyValue(MegaId propertyId, string value)
        {
            _properties[propertyId] = value;
        }

    }
}
