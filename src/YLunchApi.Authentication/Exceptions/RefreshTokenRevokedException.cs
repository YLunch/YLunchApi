using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace YLunchApi.Authentication.Exceptions;

[Serializable]
public class RefreshTokenRevokedException : Exception
{
    [ExcludeFromCodeCoverage]
    private RefreshTokenRevokedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public RefreshTokenRevokedException()
    {
    }
}
