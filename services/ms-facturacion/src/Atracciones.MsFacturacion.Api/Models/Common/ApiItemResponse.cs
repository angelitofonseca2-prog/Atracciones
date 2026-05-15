namespace Atracciones.MsFacturacion.Api.Models.Common;

public sealed class ApiItemResponse<T>
{
    public int Status { get; set; } = 200;
    public string Message { get; set; } = "Operación exitosa";
    public T? Data { get; set; }

    public ApiItemResponse() { }

    public ApiItemResponse(T data)
    {
        Data = data;
    }
}
