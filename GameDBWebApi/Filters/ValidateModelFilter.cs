using System.Linq;
using System.Text.Json;
using GameDBWebApi.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameDBWebApi.Filters
{
    /// <summary>
    /// Global action filter that converts invalid ModelState into a BadRequestException
    /// so the existing GlobalExceptionMiddleware produces a consistent response shape.
    /// </summary>
    public class ValidateModelFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var payload = JsonSerializer.Serialize(errors);

                throw new BadRequestException("validation failed", errors);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // no-op
        }
    }
}
