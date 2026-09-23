using DgiiEcf.Application.Common.Xml.XmlDocumentLoader;
using DgiiEcf.Application.Features.Documents.GetSecurityCode.Contracts;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.Domain.Documents.SecurityCode;

namespace DgiiEcf.Application.Features.Documents.GetSecurityCode;

public sealed class GetSecurityCodeHandler : IGetSecurityCodeHandler
{
    public Result<SecurityCode> Handle(GetSecurityCodeQuery query)
    {
        var loaded = XmlDocumentLoader.Load(query.SignedXml);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var signatureValue = XmlDocumentLoader.FindFirst(loaded.Value, "SignatureValue");
        if (signatureValue is null || string.IsNullOrWhiteSpace(signatureValue.Value))
        {
            return GetSecurityCodeErrors.SignatureValueNotFound;
        }

        return SecurityCode.FromSignatureValue(signatureValue.Value);
    }
}
