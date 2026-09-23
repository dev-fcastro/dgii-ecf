using System.Globalization;
using System.Security.Cryptography;
using System.Xml.Linq;
using DgiiEcf.Application.Common.Time.DominicanClock;
using DgiiEcf.Application.Common.Xml.DgiiXmlWriter;
using DgiiEcf.Application.Features.Receiver.GenerateSeed.Contracts;

namespace DgiiEcf.Application.Features.Receiver.GenerateSeed;

/// <summary>
/// Same shape DGII uses: 128 random bytes in base64 plus the Dominican local timestamp.
/// </summary>
public sealed class GenerateSeedHandler : IGenerateSeedHandler
{
    private const int SeedBytes = 128;
    private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";
    private static readonly XNamespace Xsd = "http://www.w3.org/2001/XMLSchema";

    private readonly TimeProvider _timeProvider;

    public GenerateSeedHandler(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public string Handle(GenerateSeedCommand command)
    {
        var valor = Convert.ToBase64String(RandomNumberGenerator.GetBytes(SeedBytes));
        var fecha = DominicanClock.Now(_timeProvider).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);

        var seed = new XElement(
            "SemillaModel",
            new XAttribute(XNamespace.Xmlns + "xsi", Xsi),
            new XAttribute(XNamespace.Xmlns + "xsd", Xsd),
            new XElement("valor", valor),
            new XElement("fecha", fecha));

        return DgiiXmlWriter.Write(seed);
    }
}
