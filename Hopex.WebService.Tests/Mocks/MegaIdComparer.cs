using Mega.Macro.API;
using System.Collections.Generic;

namespace Hopex.WebService.Tests.Mocks
{
    class MegaIdComparer : IComparer<MegaId>, IEqualityComparer<MegaId>
    {
        public bool Equals(MegaId x, MegaId y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(MegaId id)
        {
            var string64 = MegaIdConverter.To64(id);
            return string64.GetHashCode();
        }

        public int Compare(MegaId x, MegaId y)
        {
            var idX = MegaIdConverter.To64(x);
            var idY = MegaIdConverter.To64(y);
            return string.Compare(idX, idY);
        }
    }
}
