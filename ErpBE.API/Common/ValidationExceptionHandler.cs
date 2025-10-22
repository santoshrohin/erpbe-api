using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ErpBE.API.Common
{
    public class ValidationExceptionHandler : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ValidationException validationException)
            {
                var errors = validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );

                var response = new
                {
                    message = "Validation failed",
                    errors = errors,
                    statusCode = 400
                };

                context.Result = new BadRequestObjectResult(response);
                context.ExceptionHandled = true;
            }
        }
    }
}
