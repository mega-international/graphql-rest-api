using Mega.Macro.API;

namespace Hopex.WebService.Tests.Mocks
{
    public class MockCollectionDescription : MockMegaObject
    {
        static MegaIdComparer idComparer = new MegaIdComparer();

        public MockCollectionDescription(string collectionId)
            : this(MegaId.Create(collectionId)) { }

        public MockCollectionDescription(MegaId collectionId)
            : base(collectionId) { }

        public override string CallFunctionString(MegaId methodId, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null)
        {
            if (idComparer.Equals(methodId, "~f8pQpjMDK1SP[GetMetaPermission]"))
                return "CRUD";
            return "";
        }
    }
}
