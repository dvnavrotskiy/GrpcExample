using System.Runtime.Serialization;

namespace HttpShared.Exceptions;

[Serializable]
public class ClientBaseException : Exception
{
    public ClientBaseException()
    {

    }

    public ClientBaseException(string message) : base(message)
    {

    }

    public ClientBaseException(string message, Exception innerException) : base(message, innerException)
    {

    }

    protected ClientBaseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {

    }
}