using DgiiEcf.Application.Common.Contracts.ISignatureVerifier;
using DgiiEcf.Application.Common.Xml.XmlErrors;
using DgiiEcf.Application.Features.Documents.ValidateDocumentSignature.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Documents.ValidateDocumentSignature;

public sealed class ValidateDocumentSignatureHandler : IValidateDocumentSignatureHandler
{
    private readonly ISignatureVerifier _verifier;

    public ValidateDocumentSignatureHandler(ISignatureVerifier verifier)
    {
        _verifier = verifier;
    }

    public Result<SignatureVerification> Handle(ValidateDocumentSignatureQuery query) =>
        string.IsNullOrWhiteSpace(query.SignedXml) ? XmlErrors.Empty : _verifier.Verify(query.SignedXml);
}
