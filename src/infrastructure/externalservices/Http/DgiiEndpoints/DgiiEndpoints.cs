using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;

namespace DgiiEcf.ExternalServices.Http.DgiiEndpoints;

/// <summary>
/// DGII resources. DGII services live under <c>{host}/{TesteCF|CerteCF|eCF}/{resource}</c>; receivers
/// (emisor-receptor standard) under <c>{buyerHost}/{resource}</c>.
/// </summary>
public static class DgiiEndpoints
{
    public const string Seed = "Autenticacion/api/Autenticacion/Semilla";
    public const string ValidateSeed = "autenticacion/api/Autenticacion/ValidarSemilla";
    public const string SendInvoice = "recepcion/api/FacturasElectronicas";
    public const string SendSummary = "recepcionfc/api/recepcion/ecf";
    public const string SummaryInvoiceInquiry = "consultarfce/api/Consultas/Consulta";
    public const string CommercialApproval = "aprobacionComercial/api/AprobacionComercial";
    public const string TrackResultStatus = "consultaresultado/api/Consultas/Estado";
    public const string InquiryStatus = "consultaestado/api/Consultas/Estado";
    public const string TrackIds = "ConsultaTrackIds/api/TrackIds/Consulta";
    public const string DirectoryProduction = "consultadirectorio/api/consultas/obtenerdirectorioporrnc";
    public const string DirectoryTestAndCertification = "consultadirectorio/api/consultas/listado";
    public const string VoidRange = "anulacionrangos/api/operaciones/anularrango";

    public const string ServiceStatus = "api/estatusservicios/obtenerestatus";
    public const string ServiceMaintenance = "api/estatusservicios/obtenerventanasmantenimiento";
    public const string ServiceVerification = "api/estatusservicios/verificarestado";

    public const string ReceiverSeed = "fe/autenticacion/api/semilla";
    public const string ReceiverValidateSeed = "fe/autenticacion/api/validacioncertificado";
    public const string ReceiverInvoice = "fe/recepcion/api/ecf";
    public const string ReceiverCommercialApproval = "fe/aprobacioncomercial/api/ecf";

    public static Uri Dgii(string baseUrl, DgiiEnvironment environment, string resource) =>
        new($"{baseUrl.TrimEnd('/')}/{environment.ToPathSegment()}/{resource.TrimStart('/')}");

    public static Uri Receiver(string buyerHost, string resource) =>
        new($"{buyerHost.Trim().TrimEnd('/')}/{resource.TrimStart('/')}");

    public static Uri Status(string baseUrl, string resource) =>
        new($"{baseUrl.TrimEnd('/')}/{resource.TrimStart('/')}");

    public static string Directory(DgiiEnvironment environment) =>
        environment == DgiiEnvironment.Production ? DirectoryProduction : DirectoryTestAndCertification;
}
