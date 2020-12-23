using System.Diagnostics.CodeAnalysis;

namespace Hopex.Model.DataModel
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum AggregateNumbersFunctionTypeEnum
    {
        COUNT,
        COUNTBLANK,
        SUM,
        AVERAGE,
        MEDIAN,
        MIN,
        MAX
    }
}
