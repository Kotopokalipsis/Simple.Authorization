using Application.Common.Interfaces.Application.Responses;

namespace Application.Common.Responses
{
    public class BaseResponse<T> : IBaseResponse<T> where T : class
    {
        public int StatusCode { get; set; }
        public T Data { get; set; }
    }
}