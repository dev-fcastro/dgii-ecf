using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;

namespace DgiiEcf.Application.Features.Documents.GenerateQrCodeUrl;

/// <summary>
/// QR URL printed on an e-CF (<c>ecf.dgii.gov.do/{env}/consultatimbre</c>). <paramref name="MontoTotal"/> is
/// written as given (e.g. <c>180000.00</c>). <c>RncComprador</c> is omitted when empty or for e-CF 43 and 47.
/// </summary>
public sealed record EcfQrCodeUrlQuery(
    string RncEmisor,
    string? RncComprador,
    string Encf,
    string MontoTotal,
    string FechaEmision,
    string FechaFirma,
    string SecurityCode,
    DgiiEnvironment Environment);

/// <summary>
/// QR URL printed on a consumo invoice under RD$250,000 (<c>fc.dgii.gov.do/{env}/consultatimbrefc</c>).
/// <paramref name="MontoTotal"/> is written without trailing zeros (<c>180000.00</c> → <c>180000</c>).
/// </summary>
public sealed record FcQrCodeUrlQuery(
    string RncEmisor,
    string Encf,
    decimal MontoTotal,
    string SecurityCode,
    DgiiEnvironment Environment);
