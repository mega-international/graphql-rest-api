using Hopex.WebService.Tests.Mocks;
using Mega.Macro.API;
using Moq;

namespace Hopex.WebService.Tests.Assertions
{
    public static class MegaIdMatchers
    {
        public static MegaId IsId(MegaId id)
        {
            var comparer = new MegaIdComparer();
            return Match.Create<MegaId>(id2 => comparer.Equals(id, id2));
        }

        public static string IsIdString(string id)
        {
            var comparer = new MegaIdComparer();
            var megaId1 = MegaId.Create(id);
            return Match.Create<string>(id2 => comparer.Equals(id, MegaId.Create(id2)));
        }
    }
}
