using Hopex.Model.Abstractions;
using Mega.Macro.API.Enums;
using System.Collections.Generic;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaWizardContext : MockMegaWrapperObject, IMegaWizardContext
    {
        protected Dictionary<string, object[]> _properties = new Dictionary<string, object[]>();

        public WizardCreateMode Mode { get; set; }
        private IMegaCollection _parent;
        private readonly string _createdObjectId;

        public MockMegaWizardContext(IMegaCollection parent, string createdObjectId)
        {
            _parent = parent;
            _createdObjectId = createdObjectId;
        }
        public override void InvokePropertyPut(string property, params object[] args)
        {
            _properties[property] = args;
        }

        public T GetProperty<T>(string property)
        {
            return (T)_properties[property][0];
        }

        public object Create()
        {
            var created = _parent.Create(_createdObjectId);
            return created.Id.Value;
        }
    }
}
