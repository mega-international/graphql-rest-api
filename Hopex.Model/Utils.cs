using System.Text.RegularExpressions;
using Mega.Macro.API;

namespace Hopex.Model
{
    internal static class Utils
    {
        private static Regex _namePattern = new Regex(@"[_A-Za-z][_0-9A-Za-z]*");

        internal static MegaId SanitizeId(MegaId id)
        {
            if (id?.ToString().Length > 0 && id.ToString()[0] == '~')
            {
                return id.ToString().Substring(1);
            }
            return id;
        }

        internal static MegaId NormalizeHopexId(MegaId id)
        {
            if (id != null && id.ToString().Length > 1 && id.ToString()[0] != '~')
            {
                id = '~' + id.ToString();
            }
            return id;
        }

        internal static bool CheckName(string name)
        {
            return _namePattern.IsMatch(name);
        }
    }
}
