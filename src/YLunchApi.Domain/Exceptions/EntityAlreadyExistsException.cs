using System.Runtime.Serialization;

namespace YLunchApi.Domain.Exceptions;

[Serializable]
public sealed class EntityAlreadyExistsException : Exception
{
    public EntityAlreadyExistsException(string message = "") : base(message)
    {
    }

    private EntityAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
