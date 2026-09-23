using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Xml.XmlErrors;

public static class XmlErrors
{
    public static readonly Error Empty = new("xml.empty", "El documento XML está vacío.");

    public static Error Invalid(string detail) => new("xml.invalid", $"XML inválido: {detail}");

    public static Error ElementNotFound(string elementName) =>
        new("xml.element_not_found", $"No se encontró el elemento '{elementName}' en el XML.");

    public static Error UnexpectedRoot(string expected, string actual) =>
        new("xml.unexpected_root", $"Se esperaba el elemento raíz '{expected}' pero se encontró '{actual}'.");

    public static Error InvalidValue(string elementName, string value) =>
        new("xml.invalid_value", $"El valor '{value}' del elemento '{elementName}' no tiene el formato esperado.");

    public static readonly Error MissingRootAttribute = new(
        "xml.missing_root",
        "El tipo a serializar debe tener el atributo [XmlRoot] con el nombre del elemento raíz.");
}
