using Mega.Macro.API;
using System;

namespace Hopex.WebService.Tests.Mocks
{
    internal class MegaIdConverter
    {
        internal static string To64(MegaId id)
        {
            return ConvertIdTo64(id);
        }

        internal static string ToUnnamedField(MegaId id)
        {
            return $"~{ConvertIdTo64(id)}[X]";
        }

        internal static string ToField(MegaId id)
        {
            if (MegaIdUtils.IsMegaField(id))
                return id.ToString();
            return $"~{ConvertIdTo64(id)}[Default mock name]";
        }

        private static string ConvertIdTo64(MegaId id)
        {
            if (id.Value is string idString)
            {
                if (idString[0] == '~') return idString.Substring(1, 12);
                if (idString.Length == 12) return idString;
                if (idString.Length == 16) return Convert16To64(idString);
                throw new Exception("Malformed idabs"); ;
            }
            var idDouble = (double)id.Value;
            return Convert16To64(ConvertDoubleTo16(idDouble));
        }

        private static string Convert16To64(string idString)
        {
            var uppedId = idString.ToUpper().ToCharArray();
            int[] tcTotal = new int[16];
            for (var nCount = 1; nCount < 9; nCount += 2)
            {
                tcTotal[nCount] = 16 * CharCode16ToNum(uppedId[2 * nCount])
                    + CharCode16ToNum(uppedId[2 * nCount + 1]);
                tcTotal[nCount + 1] = 16 * CharCode16ToNum(uppedId[2 * (nCount - 1)])
                    + CharCode16ToNum(uppedId[2 * (nCount - 1) + 1]);
                tcTotal[0] = (tcTotal[0] + tcTotal[nCount] + tcTotal[nCount + 1]) % 256;
            }
            var id16 = "";
            for (var nCount = 0; nCount < 9; nCount += 3)
            {
                var ul = tcTotal[nCount]
                    + tcTotal[nCount + 1] * 256
                    + tcTotal[nCount + 2] * 65536;
                for (var nAux = 0; nAux < 4; nAux++)
                {
                    var c = ul % 64;
                    ul /= 64;
                    id16 += NumToChar64(c);
                }
            }
            return id16;
        }

        private static int CharCode16ToNum(char code)
        {
            const int NB_POSSIBLE_DIGITS = 10;
            if (code >= '0' && code <= '9') return code - '0';
            if (code >= 'A' && code <= 'F') return code - 'A' + NB_POSSIBLE_DIGITS;
            throw new Exception("Malformed heaxidabs");
        }

        private static readonly char[] _BASE64 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz()".ToCharArray();
        private static char NumToChar64(int num)
        {
            return _BASE64[num];
        }

        private static string ConvertDoubleTo16(double d)
        {
            var bytes = BitConverter.GetBytes(d);
            var reorderedBytes = new byte[] { bytes[1], bytes[0], bytes[3], bytes[2], bytes[5], bytes[4], bytes[7], bytes[6] };
            return BitConverter.ToString(reorderedBytes).Replace("-", "");
        }
    }
}
