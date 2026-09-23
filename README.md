# DgiiEcf

[![CI](https://github.com/dev-fcastro/dgii-ecf/actions/workflows/ci.yml/badge.svg)](https://github.com/dev-fcastro/dgii-ecf/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/DgiiEcf?logo=nuget)](https://www.nuget.org/packages/DgiiEcf)
[![Descargas](https://img.shields.io/nuget/dt/DgiiEcf?label=descargas)](https://www.nuget.org/packages/DgiiEcf)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Licencia: MIT](https://img.shields.io/badge/licencia-MIT-green.svg)](LICENSE)
[![Sitio](https://img.shields.io/badge/sitio-sellernet.vercel.app-004880)](https://sellernet.vercel.app/)

Facturación electrónica (e-CF) de la **DGII** de República Dominicana para **.NET 8 y .NET 10**.

Es un port del paquete Node [`dgii-ecf`](https://github.com/victors1681/dgii-ecf) (v1.8.5), organizado con Clean Architecture y el patrón `Result<T>`.

| Área | Qué incluye |
|---|---|
| Firma | XML-DSig con el perfil de la DGII: C14N inclusivo, RSA-SHA256, `Reference URI=""`, enveloped |
| Emisor | Autenticación por semilla con token renovado automáticamente, envío de e-CF, RFCE, aprobación comercial (ACECF), anulación de secuencias (ANECF) y consultas |
| Receptor | Semilla, validación de la semilla firmada con emisión de JWT, validación del token, lectura del multipart y acuse de recibo (ARECF) |
| Herramientas | Código de seguridad, conversión e-CF 32 → RFCE, URL del QR, validación de firmas, JSON ⇄ XML compatible con el paquete Node |
| Documentos tipados | `Ecf`, `Rfce`, `Arecf`, `Acecf` y `Anecf`, con el orden de elementos del XSD |

## Instalación

```bash
dotnet add package DgiiEcf
```

## Configuración

`appsettings.json`:

```json
{
  "DgiiEcf": {
    "Environment": "Test",
    "StatusApiKey": null,
    "Certificate": {
      "CertificatePath": "certs/empresa.p12",
      "CertificatePassword": "<desde un secret store>"
    }
  }
}
```

Los valores posibles de `Environment` son:

| Valor | Ambiente DGII |
|---|---|
| `Test` | TesteCF |
| `Certification` | CerteCF |
| `Production` | eCF |

El certificado también se puede entregar como base64 en `CertificateBase64`, algo útil en contenedores y secret managers. Nunca guardes la contraseña en el repositorio.

```csharp
builder.Services.AddDgiiEcf(builder.Configuration);
```

La configuración también se puede hacer por código, o partiendo de un `X509Certificate2` que ya tengas abierto:

```csharp
services.AddDgiiEcf(options =>
{
    options.Environment = DgiiEnvironment.Certification;
    options.CertificateBase64 = secrets["dgii-p12"];
    options.CertificatePassword = secrets["dgii-p12-password"];
});

services.AddDgiiEcf(certificate, options => options.Environment = DgiiEnvironment.Production);
```

Si no usas un contenedor de DI:

```csharp
using var dgii = DgiiEcfClient.Create(options => { /* ... */ });
var client = dgii.Client;     // IDgiiEcfClient
var receiver = dgii.Receiver; // IEcfReceiver
```

## Uso como emisor

Todas las operaciones devuelven `Result<T>`. Un error esperado nunca lanza excepción: se revisa `IsSuccess` o `Error`. Cuando falla la DGII, `Error` es un `DgiiApiError` que trae `Status`, `Messages` (los `mensajes` de la DGII) y `RawBody`.

```csharp
public sealed class FacturacionService(IDgiiEcfClient dgii)
{
    public async Task<string?> EnviarAsync(Ecf factura, CancellationToken ct)
    {
        var firmado = dgii.SignDocument(factura);   // o dgii.Sign(xml, "ECF")
        if (firmado.IsFailure) return null;

        // El token se obtiene y se renueva solo. Si la DGII responde 401, se reintenta una vez con un token nuevo.
        // Por defecto el archivo se nombra {RNCEmisor}{eNCF}.xml.
        var envio = await dgii.SendElectronicDocumentAsync(firmado.Value, cancellationToken: ct);
        if (envio.IsFailure)
        {
            // envio.Error.Message trae el texto de la DGII, por ejemplo "e-NCF duplicado".
            return null;
        }

        var estado = await dgii.GetTrackStatusAsync(envio.Value.TrackId!, ct);
        return estado.IsSuccess ? estado.Value.Estado : null;
    }
}
```

| Operación | Método |
|---|---|
| Autenticar (forzar) | `AuthenticateAsync(buyerHost?)` |
| Enviar e-CF a la DGII o a un receptor | `SendElectronicDocumentAsync(signedXml, fileName?, buyerHost?)` |
| Factura de consumo < RD$250,000 | `CreateSignedRfce(signedEcf32)` + `SendSummaryAsync(rfce.SignedXml)` |
| Aprobación comercial | `SendCommercialApprovalAsync(signedAcecf, fileName?, buyerHost?)` |
| Anulación de secuencias | `VoidEncfAsync(signedAnecf, fileName?)` |
| Estado por trackId | `GetTrackStatusAsync(trackId)` |
| Consulta de estado de un e-CF | `InquiryStatusAsync(rncEmisor, encf, rncComprador?, codigoSeguridad?)` |
| TrackIds de un e-NCF | `GetTrackIdsAsync(rncEmisor, encf)` |
| Directorio de facturadores | `GetCustomerDirectoryAsync(rnc)` (siempre devuelve una lista) |
| Consulta RFCE (solo producción) | `GetSummaryInvoiceInquiryAsync(rncEmisor, encf, codigoSeguridad)` |
| Estatus de servicios DGII | `GetServicesStatusAsync()`, `GetMaintenanceWindowsAsync()`, `VerifyServiceStatusAsync()` (requieren `StatusApiKey`) |

### Factura de consumo (e-CF 32 menor de RD$250,000)

```csharp
var ecf32 = dgii.SignDocument(facturaConsumo).Value;       // guárdalo: es el documento completo
var rfce = dgii.CreateSignedRfce(ecf32).Value;             // resumen firmado + código de seguridad
var respuesta = await dgii.SendSummaryAsync(rfce.SignedXml);
var qr = EcfTools.FcQrCodeUrl(rnc, encf, montoTotal, rfce.SecurityCode, DgiiEnvironment.Test);
```

## Uso como receptor (estándar emisor-receptor)

Ejemplo con Minimal API:

```csharp
app.MapGet("/fe/autenticacion/api/semilla", (IEcfReceiver r) => Results.Content(r.GenerateSeed(), "application/xml"));

app.MapPost("/fe/autenticacion/api/validacioncertificado", async (HttpRequest req, IEcfReceiver r) =>
{
    var body = await new StreamReader(req.Body).ReadToEndAsync();
    var archivo = r.ParseReceivedDocument(body, req.ContentType!);
    var token = archivo.IsSuccess ? r.ValidateSignedSeed(archivo.Value.XmlContent) : null;
    return token is { IsSuccess: true } ? Results.Ok(token.Value) : Results.Unauthorized();
});

app.MapPost("/fe/recepcion/api/ecf", async (HttpRequest req, IEcfReceiver r) =>
{
    var auth = r.ValidateToken(req.Headers.Authorization.ToString());
    if (auth.IsFailure || auth.Value.IsExpired) return Results.Unauthorized();

    var body = await new StreamReader(req.Body).ReadToEndAsync();
    var archivo = r.ParseReceivedDocument(body, req.ContentType!);
    if (archivo.IsFailure) return Results.BadRequest(archivo.Error.Message);

    // Rechaza por su cuenta los tipos 32, 41, 43, 45, 46 y 47 (código 1) y el RNC comprador ajeno (código 4).
    var arecf = r.BuildSignedReceiptAcknowledgement(archivo.Value.XmlContent, receiverRnc: "130862346");
    return arecf.IsSuccess ? Results.Content(arecf.Value, "application/xml") : Results.BadRequest(arecf.Error.Message);
});
```

## Herramientas (`EcfTools`)

`EcfTools` es una clase estática y no necesita certificado ni conexión HTTP.

| Método | Qué hace |
|---|---|
| `GetSecurityCode(signedXml)` | Devuelve los primeros 6 caracteres del `SignatureValue` |
| `ConvertEcf32ToRfce(signedEcf32)` | Genera el RFCE sin firmar |
| `EcfQrCodeUrl(...)` / `FcQrCodeUrl(...)` | Construye las URL del QR con el mismo encoding que `encodeURIComponent` |
| `Serialize(doc)` / `Deserialize<T>(xml)` | Convierte entre documentos tipados y XML |
| `JsonToXml(json, round)` / `XmlToJson(xml)` / `NormalizeJsonCasing(json)` | Mantiene compatibles los payloads JSON del paquete Node |
| `SetXmlValue`, `ExtractXmlFromBody`, `CurrentFormattedDateTime` | Utilidades de XML y fecha |

Para verificar la firma de un documento recibido, usa el handler `IValidateDocumentSignatureHandler` desde el contenedor de DI.

## Arquitectura

```
src/
  core/domain                    DgiiEcf.Domain            Result/Error, value objects (Rnc, Encf, SecurityCode), documentos tipados
  core/app                       DgiiEcf.Application       Features/<Módulo>/<Caso de uso> (Command/Query + Handler + Contracts), contratos
  infrastructure/signing         DgiiEcf.Signing           Certificado .p12, XML-DSig, verificación, JWT RS256
  infrastructure/externalservices DgiiEcf.ExternalServices HttpClient tipado, endpoints DGII, errores DGII
  packaging/sdk                  DgiiEcf                   AddDgiiEcf(), fachadas, paquete NuGet único
srcTest/                         espejo 1:1 de src/ (xUnit)
```

## Diferencias con el paquete Node

- **Token:** en Node el token era global y compartido. Aquí se guarda por audiencia (la DGII o cada receptor), se renueva un minuto antes de expirar y se reintenta una vez ante un 401. Puedes reemplazar `IAccessTokenStore` por un store distribuido.
- **Validación de la semilla (receptor):** Node solo comparaba el digest. Aquí también se verifica la firma criptográficamente.
- **Verificación de firmas:** acepta documentos firmados sin espacios en blanco y guardados indentados, como hacen las herramientas de la DGII. No valida la vigencia ni la cadena del certificado.
- **Conversión a RFCE:**
  - funciona con un solo `ImpuestoAdicional` (en Node fallaba);
  - rechaza documentos que no son tipo 32 o que llegan a RD$250,000 o más.
- **Certificado `.p12`:** se usa el certificado que tiene la clave privada, no el primero de la bolsa.
- **Servicio de estatus:** usa `https://statusecf.dgii.gov.do/api/estatusservicios/...` sin el segmento de ambiente que agregaba Node.
- **JSON ⇄ XML:** los números del JSON se escriben tal como vienen (`637.20` se mantiene como `637.20`). Node los normalizaba a `637.2`.

## Desarrollo

```bash
dotnet test DgiiEcf.slnx
```

Las pruebas contra TesteCF se saltan salvo que definas `DGII_CERT_PATH`, `DGII_CERT_PASSWORD` y `DGII_RNC_EMISOR`.

```bash
dotnet pack src/packaging/sdk -c Release
```

## Versionado y publicación

La versión del paquete sale de los tags de git mediante [MinVer](https://github.com/adamralph/minver). El tag `v1.2.3` produce la versión `1.2.3`. Los commits que no tienen tag generan versiones preliminares, por ejemplo `1.2.4-alpha.0.3`.

Hay dos workflows:

- **CI** (`.github/workflows/ci.yml`): en cada push y PR a `master` compila, ejecuta las pruebas en net8.0 y net10.0, y deja el `.nupkg` como artefacto.
- **Publish** (`.github/workflows/publish.yml`): se dispara de dos formas:
  - desde *Actions → Publish → Run workflow*, eligiendo `patch`, `minor` o `major`, con lo que crea el siguiente tag por su cuenta;
  - empujando un tag a mano, por ejemplo `git tag v1.2.3 && git push origin v1.2.3`.

  En los dos casos compila, prueba, empaqueta, publica en NuGet.org y crea el GitHub Release.

La publicación en NuGet.org usa *Trusted Publishing*, sin API keys guardadas en el repositorio. La política de nuget.org debe coincidir con estos valores: owner `dev-fcastro`, repositorio `dgii-ecf`, workflow `publish.yml`, patrón `DgiiEcf*` y usuario `fcastrodev`.

## Licencia

MIT. Basado en [`dgii-ecf`](https://github.com/victors1681/dgii-ecf) de Victor Santos (MIT).
