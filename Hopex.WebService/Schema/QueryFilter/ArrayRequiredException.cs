using System;
using System.Runtime.Serialization;

namespace Hopex.Modules.GraphQL.Schema.Filters
{
    internal class ArrayRequiredException : Exception
    {
        public ArrayRequiredException(string message): base(message)
        {
        }
    }
}
