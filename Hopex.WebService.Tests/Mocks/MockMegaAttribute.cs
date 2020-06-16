using Hopex.Model.Abstractions;
using Mega.Macro.API;
using System.Collections.Generic;
using System.Linq;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaAttribute : MockMegaWrapperObject, IMegaAttribute
    {
        private object _defaultValue;
        private Dictionary<MegaId, object> _translatedValues;

        public MockMegaAttribute()
        {
            _defaultValue = "MockMegaAttribute default value";
        }

        public MockMegaAttribute(object defaultValue, Dictionary<MegaId, object> translatedValues)
        {
            _defaultValue = defaultValue;
            _translatedValues = translatedValues;
        }

        public IMegaAttribute Translate(MegaId languageId)
        {
            var translatedValue = _translatedValues[languageId];
            return new MockMegaAttribute(translatedValue, _translatedValues);
        }

        public dynamic Value()
        {
            return _defaultValue;
        }
    }
}
