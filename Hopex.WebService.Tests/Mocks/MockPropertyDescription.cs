using Mega.Macro.API;

namespace Hopex.WebService.Tests.Mocks
{
    internal class MockPropertyDescription : MockMegaObject
    {
        public MockPropertyDescription(MegaId id) : base(id)
        {
        }

        public override string CallFunctionString(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
        {
            var idComparer = new MegaIdComparer();
            if (idComparer.Equals(methodId, "~R2mHVReGFP46[WFQuery]"))
            {
                return null;
            }
            return base.CallFunctionString(methodId, arg1, arg2, arg3, arg4, arg5, arg6);
        }
    }
}
