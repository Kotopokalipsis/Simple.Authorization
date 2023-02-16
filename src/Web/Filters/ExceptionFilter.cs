using System.Diagnostics;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;

namespace Web.Filters
{
    public class ExceptionFilter : IAsyncExceptionFilter
    {
        private readonly IWebHostEnvironment _env;

        public ExceptionFilter(IWebHostEnvironment env)
        {
            _env = env;
        }

        //TODO: add logger 
        public Task OnExceptionAsync(ExceptionContext context)
        {
            context.Result = new ObjectResult(new ErrorResponse<string>
            {
                StackTrace = _env.IsDevelopment() ? context.Exception.StackTrace : null,
                StatusCode = 500,
                ErrorStatus = ResponseErrorMessagesEnum.UnexpectedServerError.ToString(),
                ErrorMessage = _env.IsDevelopment() ? context.Exception.Message : "Unexpected Server Error"
            })
            {
                StatusCode = 500
            };

            return Task.CompletedTask;
        }
    }
}