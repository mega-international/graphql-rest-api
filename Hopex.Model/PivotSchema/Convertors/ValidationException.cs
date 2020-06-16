using System;

namespace Hopex.Model.PivotSchema.Convertors
{
    public class ValidationException : Exception
    {
        public ValidationException(ValidationContext validationContext)
        {
            ValidationContext = validationContext;
        }

        public ValidationContext ValidationContext { get; private set; }
    }
}
