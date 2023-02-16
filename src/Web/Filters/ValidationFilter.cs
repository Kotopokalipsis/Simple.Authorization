using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments.Any(v => v.Value == null))
            {
                var response = new ErrorResponse<Dictionary<string, List<string>>>()
                {
                    Errors = new Dictionary<string, List<string>>{{"ValidationError", new List<string> {"Bad request"}}},
                    StatusCode = 400,
                    ErrorMessage = ResponseErrorMessagesEnum.ValidationError.ToString()
                };
                
                context.Result = new BadRequestObjectResult(response);
                
                return;
            }
            
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .ToDictionary(x => x.Key, 
                        y => y.Value.Errors.Select(x => x.ErrorMessage).ToList());
                
                var response = new ErrorResponse<Dictionary<string, List<string>>>()
                {
                    Errors = errors,
                    StatusCode = 400,
                    ErrorMessage = ResponseErrorMessagesEnum.ValidationError.ToString()
                };
                
                context.Result = new BadRequestObjectResult(response);
                
                return;
            }
            
            await next.Invoke();
        }
    }
}