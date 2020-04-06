using Hopex.Model.Abstractions;
using System;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaWrapperObject : IMegaWrapperObject
    {
        public virtual T InvokeFunction<T>(string function, params object[] args)
        {
            throw new NotImplementedException();
        }

        public virtual void InvokePropertyPut(string property, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
