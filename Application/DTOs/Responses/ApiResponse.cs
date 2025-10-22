namespace identity_service.DTOs.Response
{
    public class ApiResponse<T> where T : class
    {
        public int Code { get; set; } = 1000;
        public string Message { get; set; }
        public T Result { get;set; }
    }
}
