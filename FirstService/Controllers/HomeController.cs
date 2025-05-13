using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SecondService.Contracts;

namespace FirstService.Controllers;

[ApiController, Route("/")]
public class HomeController(
    IConfiguration configuration,
    ILogger<HomeController> logger,
    SecondServiceClient secondServiceClient,
    SecondServiceGrpcClient secondServiceGrpcClient
    ) : ControllerBase
{
    private readonly string configName = configuration.GetValue<string>("Name") ?? "not defined";

    [HttpGet]
    public ActionResult Get()
    {
        logger.LogInformation("FirstService Home Controller executed.");
        return Ok($"FirstService ({configName}) on {Environment.MachineName} / {Process.GetCurrentProcess().StartTime}");
    }
    
    [HttpGet("second")]
    public async Task<ActionResult> GetSecond()
    {
        var status = await secondServiceClient.GetSecondHomeStatus(CancellationToken.None);
        
        logger.LogInformation("FirstService Home Controller GetSecond executed.");
        return Ok($"Second status: {status}");
    }
    
    [HttpGet("secondGrpc")]
    public async Task<ActionResult> GetSecondGrpc()
    {
        var status = await secondServiceGrpcClient.GetData("gRPC");
        
        logger.LogInformation("FirstService Home Controller GetSecondGrpc executed.");
        return Ok($"Second GRPC status: {status}");
    }
}