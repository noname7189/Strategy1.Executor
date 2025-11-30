using System.Net;

namespace Strategy1.Executor.Core.DataClass.Rest
{
    public record Response<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public string? Msg { get; set; }
        public T? Data { get; set; }
    }
}
