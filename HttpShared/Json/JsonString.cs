namespace HttpShared.Json;

public sealed class JsonString
{
    public string Value { get; }

    public JsonString(string value) => Value = value;
}