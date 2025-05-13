using Grpc.Core;
using SecondService.Services;
using SecondServiceApi;

namespace SecondService.Grpc;

public sealed class DataServiceGrpc(DataService dataService)
    : SecondServiceApiProvider.SecondServiceApiProviderBase
{
    public override Task<BasicResponse> GetBasicData(
        BasicRequest      request, 
        ServerCallContext serverCallContext
    )
    {
        var status = dataService.GetStatus(request.State);
        return Task.FromResult(new BasicResponse { StatusString = status });
    }
}