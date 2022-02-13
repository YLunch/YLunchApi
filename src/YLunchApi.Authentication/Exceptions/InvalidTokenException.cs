using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace YLunchApi.Authentication.Exceptions;

[Serializable]
public sealed class InvalidTokenException : Exception
{
    [ExcludeFromCodeCoverage]
    private InvalidTokenException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public InvalidTokenException(string message = "") : base(message)
    {
    }
}
