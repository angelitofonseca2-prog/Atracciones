using Microservicio.Atracciones.Business.DTOs.Admin.Clientes;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Business.Validators.Admin
{
    public static class ClienteAdminValidator
    {
        private static readonly string[] TiposValidos = { "CC", "RUC", "PASAPORTE", "CEDULA", "OTRO" };

        public static void ValidarCrear(CrearClienteRequest request)
        {
            var errores = new List<string>();

            if (!TiposValidos.Contains(request.TipoIdentificacion))
                errores.Add($"TipoIdentificacion inválido. Valores: {string.Join(", ", TiposValidos)}.");

            if (string.IsNullOrWhiteSpace(request.NumeroIdentificacion))
                errores.Add("El número de identificación es obligatorio.");

            // Persona natural: nombres y apellidos obligatorios
            // Empresa: razón social obligatoria
            bool esEmpresa = request.TipoIdentificacion == "RUC";
            if (esEmpresa && string.IsNullOrWhiteSpace(request.RazonSocial))
                errores.Add("La razón social es obligatoria para tipo RUC.");

            if (!esEmpresa && (string.IsNullOrWhiteSpace(request.Nombres) || string.IsNullOrWhiteSpace(request.Apellidos)))
                errores.Add("Nombres y apellidos son obligatorios para personas naturales.");

            if (string.IsNullOrWhiteSpace(request.Correo))
                errores.Add("El correo electrónico es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Login) || request.Login.Length < 4)
                errores.Add("El login debe tener al menos 4 caracteres.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                errores.Add("La contraseña debe tener al menos 8 caracteres.");

            if (errores.Any())
                throw new ValidationException(errores);
        }
    }
}
