using HttpShared;

namespace SecondService.Contracts;

public sealed class SecondServiceClient (
    HttpClient client,
    bool usePascalCaseSerialization = false
) : ClientBase(client, usePascalCaseSerialization)
{
    public async Task<string> GetSecondHomeStatus(CancellationToken ct)
        => await Get<string>(
            "",
            ct
        );
}