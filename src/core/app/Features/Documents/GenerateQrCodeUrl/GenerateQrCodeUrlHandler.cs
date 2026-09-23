using DgiiEcf.Application.Common.Text.JsNumber;
using DgiiEcf.Application.Common.Text.UriComponent;
using DgiiEcf.Application.Features.Documents.GenerateQrCodeUrl.Contracts;
using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;

namespace DgiiEcf.Application.Features.Documents.GenerateQrCodeUrl;

public sealed class GenerateQrCodeUrlHandler : IGenerateQrCodeUrlHandler
{
    public const string EcfHost = "https://ecf.dgii.gov.do";
    public const string FcHost = "https://fc.dgii.gov.do";

    public string Handle(EcfQrCodeUrlQuery query)
    {
        var buyer = !string.IsNullOrEmpty(query.RncComprador) && !OmitsBuyer(query.Encf)
            ? $"RncComprador={UriComponent.Encode(query.RncComprador)}&"
            : string.Empty;

        return $"{EcfHost}/{Segment(query.Environment)}/consultatimbre"
            + $"?rncemisor={UriComponent.Encode(query.RncEmisor)}"
            + $"&{buyer}encf={UriComponent.Encode(query.Encf)}"
            + $"&fechaemision={UriComponent.Encode(query.FechaEmision)}"
            + $"&montototal={UriComponent.Encode(query.MontoTotal)}"
            + $"&fechafirma={UriComponent.Encode(query.FechaFirma)}"
            + $"&codigoseguridad={UriComponent.Encode(query.SecurityCode)}";
    }

    public string Handle(FcQrCodeUrlQuery query) =>
        $"{FcHost}/{Segment(query.Environment)}/consultatimbrefc"
        + $"?rncemisor={UriComponent.Encode(query.RncEmisor)}"
        + $"&encf={UriComponent.Encode(query.Encf)}"
        + $"&montototal={UriComponent.Encode(JsNumber.Format(query.MontoTotal))}"
        + $"&codigoseguridad={UriComponent.Encode(query.SecurityCode)}";

    private static string Segment(DgiiEnvironment environment) => environment.ToPathSegment().ToLowerInvariant();

    // e-CF 43 (gastos menores) and 47 (pagos al exterior) have no buyer RNC.
    private static bool OmitsBuyer(string? encf) =>
        encf is not null
        && (encf.Contains("E43", StringComparison.OrdinalIgnoreCase) || encf.Contains("E47", StringComparison.OrdinalIgnoreCase));
}
