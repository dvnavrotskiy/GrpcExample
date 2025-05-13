using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HttpShared.Json
{
    public partial class JsonSerializer
    {
        private readonly JsonSerializerSettings _options;

        private JsonSerializer(JsonSerializerSettings options)
        {
            _options = options;
        }

        public string Serialize<T>(T value) where T : class
            => JsonConvert.SerializeObject(value, _options);

        public T? Deserialize<T>(string json) where T : class
            => JsonConvert.DeserializeObject<T>(json, _options);
    }

    public partial class JsonSerializer
    {
        public static JsonSerializer UsePascalCase { get; }
            = new(
                new JsonSerializerSettings
                {
                    Formatting        = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                }
            );

        public static JsonSerializer Default { get; }
            = new(
                new JsonSerializerSettings
                {
                    Formatting        = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver  = new CamelCasePropertyNamesContractResolver()
                }
            );
    }
}