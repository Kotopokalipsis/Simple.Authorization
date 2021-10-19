using System.Collections.Generic;

namespace Application.Common.Interfaces.Application.Responses
{
    public interface IErrorResponse
    {
        Dictionary<string, List<string>> Errors { get; set; }
        string StackTrace { get; set; }
        string ErrorMessage { get; set; }
        string ErrorStatus { get; set; }
    }
}