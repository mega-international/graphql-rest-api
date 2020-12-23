namespace Hopex.Modules.GraphQL.Dataset
{
    public enum MetaAttributeFormat
    {
        Standard = 'S',
        Currency = 'C',
        Utf8 = '8',
        RgbColor = 'B',
        Enumeration = 'F',
        EnumerationOpened = 'T',
        Duration = 'D',
        Percent = 'P',
        Double = 'E',
        Object = 'O',
        SignedNumber = 'Z'
    }

    public static class MetaAttributeFormatUtils
    {
        public static MetaAttributeFormat From(string format)
        {
            return CharEnumUtils.ToCharEnum<MetaAttributeFormat>(format);
        }
    }
}
