using Hopex.Model.Abstractions;
using Mega.Macro.API;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaItem : MockMegaWrapperObject, IMegaItem
    {
        public virtual IMegaRoot Root { get; set; }

        public virtual T CallFunction<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null) where T : IMegaWrapperObject
        {
            return default(T);
        }

        public virtual string CallFunctionString(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
        {
            var idComparer = new MegaIdComparer();
            if (idComparer.Equals(methodId, "~R2mHVReGFP46[WFQuery]"))
            {
                return "CRUD";
            }
            return "";
        }

        public virtual T CallFunctionValue<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null) where T : struct
        {
            return default(T);
        }
    }
}
