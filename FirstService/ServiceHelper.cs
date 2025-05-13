using SecondService.Contracts;

namespace FirstService;

public static class ServiceHelper
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration cfg)
    {
        var secondSvcBaseUri = cfg["SecondService"];
        if (string.IsNullOrEmpty(secondSvcBaseUri))
            throw new Exception("appsettings.json must define SecondService as uri to SecondService REST API");
        
        var secondSvcGrpcUri = cfg["SecondServiceGrpc"];
        if (string.IsNullOrEmpty(secondSvcGrpcUri))
            throw new Exception("appsettings.json must define SecondServiceGrpc as uri to SecondService GRPC API");


        services
           .AddSingleton(new SecondServiceClient(new HttpClient {BaseAddress = new Uri(secondSvcBaseUri)}))
           .AddSingleton(new SecondServiceGrpcClient(secondSvcGrpcUri))
            ;
        
        return services;
    }
}