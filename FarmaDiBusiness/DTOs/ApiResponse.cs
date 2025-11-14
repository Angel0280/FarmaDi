namespace FarmaDiBusiness.DTOs
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }

        public object? Meta { get; set; }
    }
}
