using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Reception.Common;

public static class ReceptionErrors
{
    public static readonly Error EmptyDocument = new("reception.empty_document", "El documento firmado es requerido.");

    public static readonly Error UnsignedDocument = new(
        "reception.unsigned_document",
        "El documento no contiene la firma digital (elemento Signature). Fírmelo antes de enviarlo.");
}
