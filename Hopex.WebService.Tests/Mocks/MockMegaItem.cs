using Hopex.Model.Mocks;
using Mega.Macro.API;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockMegaItem : MockMegaWrapperObject, IMegaItem
    {
        public virtual IMegaRoot Root { get; set; }

        public virtual T CallFunction<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null) where T : IMegaWrapperObject
        {
            return default;
        }

        public virtual string CallFunctionString(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
        {
            return "";
        }

        public virtual T CallFunctionValue<T>(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null) where T : struct
        {
            return default;
        }
    }
}
