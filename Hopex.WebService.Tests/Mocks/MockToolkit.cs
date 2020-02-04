using Hopex.Model.Mocks;
using Mega.Macro.API;

namespace Hopex.WebService.Tests.Mocks
{
    internal class MockToolkit : IMegaToolkit
    {
        public bool IsSameId(MegaId objectId1, MegaId objectId2)
        {
            return new MegaIdComparer().Equals(objectId1, objectId2);
        }
    }
}
