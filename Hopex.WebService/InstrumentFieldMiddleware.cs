using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.Types;
using System;
using System.Threading.Tasks;

namespace Hopex.Modules.GraphQL
{
    public class InstrumentFieldMiddleware : IFieldMiddleware
    {
        public async Task<object> Resolve(IResolveFieldContext context, FieldMiddlewareDelegate next)
        {
            try
            {
                return await next(context);
            }
            catch(ExecutionError)
            {
                throw;
            }
            catch(Exception ex)
            {
                var message = $"Error trying to resolve: {context?.FieldAst?.Alias ?? context?.FieldDefinition.Name ?? "null"}. Error message: {ex.Message} See more details in log file";
                throw new ExecutionError(message, ex);
            }
        }
    }
}
