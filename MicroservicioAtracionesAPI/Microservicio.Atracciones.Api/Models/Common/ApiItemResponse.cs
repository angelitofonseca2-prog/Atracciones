namespace Microservicio.Atracciones.Api.Models.Common
{
    public class ApiItemResponse<T>
    {
        public int Status { get; set; } = 200;
        public string Message { get; set; } = "Operación exitosa";
        public T? Data { get; set; }

        public ApiItemResponse() { }

        public ApiItemResponse(T data)
        {
            Data = data;
        }

        /// <summary>
        /// Permite que el campo <see cref="Status"/> del envelope coincida con el HTTP (p. ej. 201 Created).
        /// </summary>
        public ApiItemResponse(T data, int status, string? message = null)
        {
            Data = data;
            Status = status;
            if (message is not null)
                Message = message;
        }
    }
}
