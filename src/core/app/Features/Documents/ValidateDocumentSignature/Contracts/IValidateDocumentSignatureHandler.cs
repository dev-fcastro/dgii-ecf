using DgiiEcf.Application.Common.Contracts.ISignatureVerifier;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Documents.ValidateDocumentSignature.Contracts;

public interface IValidateDocumentSignatureHandler
{
    Result<SignatureVerification> Handle(ValidateDocumentSignatureQuery query);
}
