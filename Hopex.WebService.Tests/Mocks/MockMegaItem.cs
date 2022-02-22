using Hopex.Model.Abstractions;
using Mega.Macro.API;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaItem : MockMegaWrapperObject, IMegaItem
    {
        private string _objectCrud = "CRUD";

        private MockMegaObject _typeObject;

        public virtual IMegaRoot Root { get; set; }

        protected MockMegaItem()
        {

        }

        protected MockMegaItem(MockMegaItem toClone)
        {
            _objectCrud = toClone._objectCrud;
            _typeObject = toClone._typeObject;
            Root = toClone.Root;
        }

        internal MockMegaItem WithObjectCrud(string crud)
        {
            _objectCrud = crud;
            return this;
        }

        internal MockMegaItem WithTypeObject(MockMegaObject typeObject)
        {
            _typeObject = typeObject;
            return this;
        }

        public virtual T CallFunction<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null) where T : IMegaWrapperObject
        {
            return default(T);
        }

        public virtual string CallFunctionString(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
        {
            var idComparer = new MegaIdComparer();
            if (idComparer.Equals(methodId, "~R2mHVReGFP46[WFQuery]"))
            {
                if (arg1 != null && arg1.ToString() == "CRUD") return _objectCrud;
                return "CRUD";
            }
            return "";
        }

        public virtual T CallFunctionValue<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null) where T : struct
        {
            return default(T);
        }

        public dynamic CallFunction(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
        {
            return null;
        }

        public virtual bool ConditionEvaluate(MegaId methodId)
        {
            var idComparer = new MegaIdComparer();
            if (idComparer.Equals(methodId, "~PuC7Fh2WKv1H[Is Full Text Search Activated]"))
            {
                return false;
            }
            throw new System.NotImplementedException();
        }

        public IMegaObject GetTypeObject()
        {
            if (_typeObject == null) _typeObject = new MockMegaObject();
            return _typeObject;
        }
    }
}
