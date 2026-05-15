using System.ComponentModel.DataAnnotations;
using Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;
using Atracciones.MsAtracciones.Business.Exceptions;
using DomainValidationException = Atracciones.MsAtracciones.Business.Exceptions.ValidationException;

namespace Atracciones.MsAtracciones.Business.Validation;

public static class AtraccionPublicValidator
{
    public static void Validar(AtraccionFiltroRequest request)
    {
        var ctx = new ValidationContext(request);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, ctx, results, validateAllProperties: true))
            throw new DomainValidationException(results.Select(r => r.ErrorMessage ?? r.MemberNames.FirstOrDefault() ?? "inválido").ToList());
    }
}
