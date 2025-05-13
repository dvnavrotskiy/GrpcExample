using System.Runtime.Serialization;

namespace HttpShared.Exceptions;

[Serializable]
public class ClientBaseDeserializationException : ClientBaseException
{
    public ClientBaseDeserializationException()
    {

    }

    public ClientBaseDeserializationException(string message) : base(message)
    {

    }

    public ClientBaseDeserializationException(string message, Exception innerException) : base(message, innerException)
    {

    }

    protected ClientBaseDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {

    }

    public string? JsonContent { get; internal set; }
}