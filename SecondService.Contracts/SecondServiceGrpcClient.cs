using Grpc.Net.Client;
using SecondServiceApi;

namespace SecondService.Contracts;

public sealed class SecondServiceGrpcClient
{
    private readonly SecondServiceApiProvider.SecondServiceApiProviderClient client;

    public SecondServiceGrpcClient(string baseAddress)
    {
        var httpHandler = new HttpClientHandler
                          {
                              ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                          };

        var channel = GrpcChannel.ForAddress(
            baseAddress,
            new GrpcChannelOptions {HttpHandler = httpHandler}
        );

        client = new SecondServiceApiProvider.SecondServiceApiProviderClient(channel);
    }

    public async Task<string> GetData(string state)
    {
        var result = await client.GetBasicDataAsync(new BasicRequest {State = state});
        return result.StatusString;
    }
}