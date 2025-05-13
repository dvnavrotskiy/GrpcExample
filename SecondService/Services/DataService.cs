using System.Diagnostics;

namespace SecondService.Services;

public sealed class DataService(IConfiguration cfg)
{
    private readonly string configName = cfg.GetValue<string>("Name") ?? "not defined";

    public string GetStatus(string requestState)
        => $"SecondService ({configName}) on {Environment.MachineName} / {Process.GetCurrentProcess().StartTime}" +
           $", call state: {requestState}";
}