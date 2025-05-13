using System.Net;
using System.Text;
using HttpShared.Exceptions;
using HttpShared.Json;

namespace HttpShared;

public abstract class ClientBase (
    HttpClient client,
    bool usePascalCaseSerialization = false
)
{
    private readonly HttpClient client = client ?? throw new ArgumentNullException(nameof(client));

    private readonly JsonSerializer serializer = usePascalCaseSerialization
        ? JsonSerializer.UsePascalCase
        : JsonSerializer.Default;

    protected Task<TOut> Get<TOut>(string endpoint) where TOut : class
    {
        return Get<TOut>(endpoint, CancellationToken.None);
    }

    protected Task<TOut> Get<TOut>(string endpoint, TOut _) where TOut : class
    {
        return Get<TOut>(endpoint, CancellationToken.None);
    }

    protected Task<TOut> Get<TOut>(string endpoint, TOut _, CancellationToken ct) where TOut : class
    {
        return Get<TOut>(endpoint, ct);
    }

    protected async Task<TOut> Get<TOut>(string endpoint, CancellationToken ct) where TOut : class
    {
        try
        {
            var request     = BuildRequest(HttpMethod.Get, endpoint);
            var response    = await client.SendAsync(request, ct);
            var jsonContent = await HandleResponseThrowErrorIfFailed(response, endpoint);

            return Deserialize<TOut>(jsonContent);
        }
        catch (ClientBaseDeserializationException ex)
        {
            throw new ClientBaseException($"Ошибка десериализации результата запроса. Endpoint: {GetAbsoluteUri(endpoint)}{Environment.NewLine}{ex.JsonContent}", ex);
        }
        catch (Exception ex)
        {
            throw new ClientBaseException($"Ошибка отправки GET запроса. Endpoint: {GetAbsoluteUri(endpoint)}", ex);
        }
    }

    protected Task Post<TIn>(string endpoint, TIn value) where TIn : class
    {
        return Post(endpoint, value, CancellationToken.None);
    }

    protected async Task Post<TIn>(string endpoint, TIn value, CancellationToken ct) where TIn : class
    {
        try
        {
            var request  = BuildRequest(HttpMethod.Post, endpoint, Serialize(value));
            var response = await client.SendAsync(request, ct);
            _ = await HandleResponseThrowErrorIfFailed(response, endpoint);
        }
        catch (Exception ex)
        {
            throw new ClientBaseException($"Ошибка отправки POST запроса. Endpoint: {GetAbsoluteUri(endpoint)}", ex);
        }
    }

    protected async Task<TOut> Post<TOut>(string endpoint, CancellationToken ct) where TOut : class
    {
        try
        {
            var request     = BuildRequest(HttpMethod.Post, endpoint);
            var response    = await client.SendAsync(request, ct);
            var jsonContent = await HandleResponseThrowErrorIfFailed(response, endpoint);

            return Deserialize<TOut>(jsonContent);
        }
        catch (ClientBaseDeserializationException ex)
        {
            throw new ClientBaseException($"Ошибка десериализации результата запроса. Endpoint: {GetAbsoluteUri(endpoint)}{Environment.NewLine}{ex.JsonContent}", ex);
        }
        catch (Exception ex)
        {
            throw new ClientBaseException($"Ошибка отправки POST запроса. Endpoint: {GetAbsoluteUri(endpoint)}", ex);
        }
    }

    protected Task<TOut> Post<TIn, TOut>(string endpoint, TIn value) where TIn : class where TOut : class
    {
        return Post<TIn, TOut>(endpoint, value, CancellationToken.None);
    }

    protected async Task<TOut> Post<TIn, TOut>(string endpoint, TIn value, CancellationToken ct) where TIn : class where TOut : class
    {
        try
        {
            var request     = BuildRequest(HttpMethod.Post, endpoint, Serialize(value));
            var response    = await client.SendAsync(request, ct);
            var jsonContent = await HandleResponseThrowErrorIfFailed(response, endpoint);

            return Deserialize<TOut>(jsonContent);
        }
        catch (KeyNotFoundException) // Возможно никогда не возникнет
        {
            throw;
        }
        catch (ClientBaseDeserializationException ex)
        {
            throw new ClientBaseException($"Ошибка десериализации результата запроса. Endpoint: {GetAbsoluteUri(endpoint)}{Environment.NewLine}{ex.JsonContent}", ex);
        }
        catch (Exception ex)
        {
            throw new ClientBaseException($"Ошибка отправки POST запроса. Endpoint: {GetAbsoluteUri(endpoint)}", ex);
        }
    }
        
    protected async Task<TOut> Post<TIn, TOut>(string endpoint, TIn value, Dictionary<string, string> headers, CancellationToken ct) where TIn : class where TOut : class
    {
        try
        {
            var request = BuildRequest(HttpMethod.Post, endpoint, Serialize(value));
            AddHeaders(request, headers);
            var response    = await client.SendAsync(request, ct);
            var jsonContent = await HandleResponseThrowErrorIfFailed(response, endpoint);

            return Deserialize<TOut>(jsonContent);
        }
        catch (KeyNotFoundException) // Возможно никогда не возникнет
        {
            throw;
        }
        catch (ClientBaseDeserializationException ex)
        {
            throw new ClientBaseException($"Ошибка десериализации результата запроса. Endpoint: {GetAbsoluteUri(endpoint)}{Environment.NewLine}{ex.JsonContent}", ex);
        }
        catch (Exception ex)
        {
            throw new ClientBaseException($"Ошибка отправки POST запроса. Endpoint: {GetAbsoluteUri(endpoint)}", ex);
        }
    }

    protected Task<TOut> Post<TIn, TOut>(string endpoint, TIn value, TOut _) where TIn : class where TOut : class
    {
        return Post<TIn, TOut>(endpoint, value, CancellationToken.None);
    }

    protected async Task<TOut> Post<TIn, TOut>(string endpoint, TIn value, TOut _, CancellationToken ct)
        where TIn : class where TOut : class
    {
        try
        {
            var body        = Serialize(value);
            var request     = BuildRequest(HttpMethod.Post, endpoint, body);
            var response    = await client.SendAsync(request, ct);
            var jsonContent = await HandleResponseThrowErrorIfFailed(response, endpoint);

            return Deserialize<TOut>(jsonContent);
        }
        catch (ClientBaseDeserializationException ex)
        {
            throw new ClientBaseException($"Ошибка десериализации результата запроса. Endpoint: {GetAbsoluteUri(endpoint)}{Environment.NewLine}{ex.JsonContent}", ex);
        }
        catch (Exception ex)
        {
            throw new ClientBaseException($"Ошибка отправки POST запроса. Endpoint: {GetAbsoluteUri(endpoint)}", ex);
        }
    }

    protected HttpRequestMessage BuildRequest(HttpMethod method, string endpoint)
        => new (method, Endpoint(endpoint));

    protected HttpRequestMessage BuildRequest(HttpMethod method, string endpoint, string jsonContent)
        => new (method, Endpoint(endpoint))
           {
               Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
           };

    protected static void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers == null)
            return;
        foreach (var pair in headers)
            request.Headers.Add(pair.Key, pair.Value);
    }
        
    private string Serialize<T>(T value) where T : class
    {
        if (value is JsonString x)
        {
            return x.Value;
        }

        return serializer.Serialize(value);
    }

    static T? Deserialize<T>(string jsonContent) where T : class
    {
        if (typeof(T) == typeof(string))
            return jsonContent as T;

        return Deserialize<T>(jsonContent, false);
    }

    private static T? Deserialize<T>(string jsonContent, bool pascalCase) where T : class
    {
        var errorMessage = $"Ошибка десериализации в объект типа {typeof(T)}";

        if (typeof(T) == typeof(string))
            return jsonContent as T;

        try
        {
            var result = pascalCase
                ? JsonSerializer.UsePascalCase.Deserialize<T>(jsonContent)
                : JsonSerializer.Default.Deserialize<T>(jsonContent);

            if (result == null)
            {
                throw new ArgumentException(nameof(result));
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new ClientBaseDeserializationException(errorMessage, ex)
                  {
                      JsonContent = jsonContent
                  };
        }
    }

    protected string Endpoint(string value, params object[] args)
    {
        if (client.BaseAddress.AbsolutePath.EndsWith("/"))
            value = new string(value.SkipWhile(c => c == '/').ToArray());

        return args.Any()
            ? string.Format(value, args)
            : value;
    }

    protected string GetAbsoluteUri(string endpoint)
    {
        return string.Concat(
            client.BaseAddress.AbsoluteUri
                   .TrimEnd('/')
                   .Append('/')
                   .Concat(endpoint.SkipWhile(c => c == '/'))
        );
    }

    private async Task<string?> HandleResponseThrowErrorIfFailed(HttpResponseMessage response, string endpoint)
    {
        var code         = response.StatusCode;
        var jsonContent  = await TryGetContentOrDefault(response.Content);
        var errorMessage = GetExceptionMessage(jsonContent ?? "No content", GetTraceIfAvailable() ?? "null");

        if (code == HttpStatusCode.OK && jsonContent != null)
            return jsonContent;

        throw new Exception(errorMessage);

        async Task<string?> TryGetContentOrDefault(HttpContent content)
        {
            try
            {
                return await content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }

        string GetExceptionMessage(string content, string trace)
        {
            return
                new StringBuilder()
                   .Append((int) code).Append(' ').AppendLine(code.ToString())
                   .Append("Failed for ").AppendLine(GetAbsoluteUri(endpoint))
                   .Append("trace-id: ").AppendLine(trace)
                   .AppendLine()
                   .AppendLine(content)
                   .ToString();
        }

        string? GetTraceIfAvailable()
        {
            try
            {
                foreach (var header in response.Headers)
                {
                    if (header.Key.Equals("trace-id", StringComparison.OrdinalIgnoreCase))
                    {
                        if (header.Value.Any())
                        {
                            var value = string.Join(string.Empty, header.Value);

                            return string.IsNullOrWhiteSpace(value) ? null : value;
                        }

                        return null;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            } 
        }
    }
        
    protected static Request<T0> GetRequest<T0>(T0 arg0) => new Request<T0> { Data = arg0 };
    protected static Request<T0, T1> GetRequest<T0, T1>(T0 arg0, T1 arg1) => new Request<T0, T1> { Arg0 = arg0, Arg1 = arg1 };
}

public class Request<T>
{
    public T Data { get; set; }
}

public class Request<T1, T2>
{
    public T1 Arg0 { get; set; }
    public T2 Arg1 { get; set; }
}
