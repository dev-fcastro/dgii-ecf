using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Authentication.Authenticate;

public static class AuthenticateErrors
{
    public static readonly Error EmptySeed = new("authentication.empty_seed", "El servicio de autenticación no devolvió la semilla.");

    public static readonly Error EmptyToken = new("authentication.empty_token", "El servicio de autenticación no devolvió un token.");
}
