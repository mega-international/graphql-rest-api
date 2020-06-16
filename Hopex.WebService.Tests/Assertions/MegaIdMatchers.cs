using Hopex.WebService.Tests.Mocks;
using Mega.Macro.API;
using Moq;
using System;

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

        public static object IsIdObject(object id)
        {
            var id1 = id as MegaId ?? MegaId.Create(id);
            var comparer = new MegaIdComparer();
            return Match.Create<object>(o2 =>
            {
                var id2 = o2 as MegaId ?? MegaId.Create(o2);
                try
                {
                    return comparer.Equals(id1, id2);
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        public static MegaId IsMegaId(MegaId id)
        {
            var comparer = new MegaIdComparer();
            return Match.Create<MegaId>(id2 => comparer.Equals(id, id2));
        }
    }
}
