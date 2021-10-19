using System.Collections.Generic;
using Application.Common.Interfaces.Application.Responses;

namespace Application.Common.Responses
{
    public class RollbackTransactionErrorResponse<T> : BaseResponse<T>, IRollbackTransactionResponse, IErrorResponse where T : class
    {
        public string StackTrace { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStatus { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }
    }
}