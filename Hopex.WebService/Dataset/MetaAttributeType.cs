namespace Hopex.Modules.GraphQL.Dataset
{
    public enum MetaAttributeType
    {
        String = 'X',
        Boolean = '1',
        Short = 'S',
        Long = 'L',
        DateTime = 'D',
        VarChar = 'A',
        VarBinary = 'B',
        Binary = 'Q',
        MegaIdentifier = 'H',
        Float = 'F',
        Currency = 'C',
        DateTime64 = 'W',
        AbsoluteDateTime64 = 'U'
    }

    public static class MetaAttributeTypeUtils
    {
        public static MetaAttributeType From(string type)
        {
            return CharEnumUtils.ToCharEnum<MetaAttributeType>(type);
        }
    }
}
