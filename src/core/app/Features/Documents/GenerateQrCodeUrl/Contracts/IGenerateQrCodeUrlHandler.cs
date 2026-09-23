namespace DgiiEcf.Application.Features.Documents.GenerateQrCodeUrl.Contracts;

public interface IGenerateQrCodeUrlHandler
{
    string Handle(EcfQrCodeUrlQuery query);

    string Handle(FcQrCodeUrlQuery query);
}
