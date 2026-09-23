using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Domain.Documents.SecurityCode;

public static class SecurityCodeErrors
{
    public static readonly Error InvalidLength = new(
        "security_code.invalid_length",
        "El código de seguridad debe tener exactamente 6 caracteres.");
}
