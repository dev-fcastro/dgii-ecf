using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Documents.GetSecurityCode;

public static class GetSecurityCodeErrors
{
    public static readonly Error SignatureValueNotFound = new(
        "security_code.signature_value_not_found",
        "No se encontró SignatureValue en el documento; el documento debe estar firmado.");
}
