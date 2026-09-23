using System.Text;
using System.Text.RegularExpressions;
using DgiiEcf.Application.Features.Receiver.Common;
using DgiiEcf.Application.Features.Receiver.ParseReceivedDocument.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Receiver.ParseReceivedDocument;

/// <summary>
/// Minimal RFC 7578 reader: returns the first part that carries a <c>filename</c>. Accepts CRLF (standard)
/// and bare LF line breaks, quoted or unquoted parameters.
/// </summary>
public sealed class ParseReceivedDocumentHandler : IParseReceivedDocumentHandler
{
    private static readonly Regex BoundaryPattern = new(
        "boundary=(?:\"(?<value>[^\"]+)\"|(?<value>[^;\\s]+))",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex FileNamePattern = new(
        "filename=(?:\"(?<value>[^\"]*)\"|(?<value>[^;\\s]+))",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public Result<ReceivedDocument> Handle(ParseReceivedDocumentCommand command)
    {
        if (string.IsNullOrEmpty(command.Body))
        {
            return ReceiverErrors.EmptyBody;
        }

        byte[] body;
        if (command.IsBase64Encoded)
        {
            try
            {
                body = Convert.FromBase64String(command.Body);
            }
            catch (FormatException)
            {
                return ReceiverErrors.InvalidBase64Body;
            }
        }
        else
        {
            body = Encoding.UTF8.GetBytes(command.Body);
        }

        return Handle(body, command.ContentType);
    }

    public Result<ReceivedDocument> Handle(ReadOnlySpan<byte> body, string contentType)
    {
        if (body.IsEmpty)
        {
            return ReceiverErrors.EmptyBody;
        }

        var boundaryMatch = BoundaryPattern.Match(contentType ?? string.Empty);
        if (!boundaryMatch.Success)
        {
            return ReceiverErrors.MissingBoundary;
        }

        var delimiter = Encoding.ASCII.GetBytes("--" + boundaryMatch.Groups["value"].Value);
        var offset = body.IndexOf(delimiter);
        while (offset >= 0)
        {
            var part = body[(offset + delimiter.Length)..];
            if (part.StartsWith("--"u8))
            {
                break;
            }

            var (headersEnd, separatorLength) = FindHeadersEnd(part);
            if (headersEnd < 0)
            {
                break;
            }

            var headers = Encoding.UTF8.GetString(part[..headersEnd]);
            var content = part[(headersEnd + separatorLength)..];
            var nextDelimiter = content.IndexOf(delimiter);
            if (nextDelimiter < 0)
            {
                break;
            }

            var fileName = FileNamePattern.Match(headers);
            var value = TrimTrailingLineBreak(content[..nextDelimiter]);
            if (fileName.Success && !value.IsEmpty)
            {
                return new ReceivedDocument(fileName.Groups["value"].Value, Encoding.UTF8.GetString(value));
            }

            offset = (int)(body.Length - content.Length) + nextDelimiter;
        }

        return ReceiverErrors.IncompleteFormData;
    }

    private static (int Index, int Length) FindHeadersEnd(ReadOnlySpan<byte> part)
    {
        var crlf = part.IndexOf("\r\n\r\n"u8);
        var lf = part.IndexOf("\n\n"u8);
        if (crlf >= 0 && (lf < 0 || crlf <= lf))
        {
            return (crlf, 4);
        }

        return lf >= 0 ? (lf, 2) : (-1, 0);
    }

    private static ReadOnlySpan<byte> TrimTrailingLineBreak(ReadOnlySpan<byte> value)
    {
        if (value.EndsWith("\r\n"u8))
        {
            return value[..^2];
        }

        return value.EndsWith("\n"u8) ? value[..^1] : value;
    }
}
