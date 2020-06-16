using Hopex.Model.Abstractions;
using System.Collections.Generic;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaWizardContext : MockMegaWrapperObject, IMegaWizardContext
    {
        protected Dictionary<string, object[]> _properties = new Dictionary<string, object[]>();

        public override void InvokePropertyPut(string property, params object[] args)
        {
            _properties[property] = args;
        }

        public T GetProperty<T>(string property)
        {
            return (T)_properties[property][0];
        }
    }
}
