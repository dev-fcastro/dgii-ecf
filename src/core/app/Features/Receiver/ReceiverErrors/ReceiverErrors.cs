using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Receiver.ReceiverErrors;

public static class ReceiverErrors
{
    public static readonly Error NotASeed = new("receiver.not_a_seed", "El documento no es una semilla (se esperaba el elemento raíz SemillaModel).");

    public static readonly Error SeedValueMissing = new("receiver.seed_value_missing", "La semilla no contiene el elemento 'valor'.");

    public static readonly Error InvalidSeedSignature = new(
        "receiver.invalid_seed_signature",
        "La firma de la semilla no es válida o el documento fue alterado.");

    public static readonly Error EmptyToken = new("receiver.empty_token", "El token es requerido.");

    public static readonly Error EmptyBody = new("receiver.empty_body", "El cuerpo de la solicitud está vacío.");

    public static readonly Error InvalidBase64Body = new("receiver.invalid_base64_body", "El cuerpo de la solicitud no es base64 válido.");

    public static readonly Error MissingBoundary = new(
        "receiver.missing_boundary",
        "El Content-Type debe ser multipart/form-data e incluir el boundary.");

    public static readonly Error IncompleteFormData = new(
        "receiver.incomplete_form_data",
        "El formulario no contiene un archivo XML (campo 'xml').");

    public static Error MissingElement(string elementName) =>
        new("receiver.missing_element", $"El e-CF recibido no contiene el elemento '{elementName}'.");
}
