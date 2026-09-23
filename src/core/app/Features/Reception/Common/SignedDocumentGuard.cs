using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Reception.Common;

/// <summary>
/// Checks performed before sending any document: not empty and already signed.
/// </summary>
public static class SignedDocumentGuard
{
    public static Result<string> Check(string? signedXml, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(signedXml))
        {
            return ReceptionErrors.EmptyDocument;
        }

        if (!signedXml.Contains("SignatureValue", StringComparison.Ordinal))
        {
            return ReceptionErrors.UnsignedDocument;
        }

        return DocumentFileName.Resolve(signedXml, fileName);
    }
}
