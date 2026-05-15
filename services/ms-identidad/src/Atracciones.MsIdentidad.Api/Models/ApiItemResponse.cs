namespace Atracciones.MsIdentidad.Api.Models;

public sealed class ApiItemResponse<T>
{
    public int Status { get; set; } = 200;
    public string Message { get; set; } = "Operación exitosa";
    public T? Data { get; set; }

    public ApiItemResponse() { }

    public ApiItemResponse(T data) => Data = data;

    public ApiItemResponse(T data, int status, string? message = null)
    {
        Data = data;
        Status = status;
        if (message is not null)
            Message = message;
    }
}
