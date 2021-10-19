namespace Application.Common.Interfaces.Application.Responses
{
    public interface IBaseResponse<T> where T : class
    {
        public int StatusCode { get; set; }
        public T Data { get; set; }
    }
}