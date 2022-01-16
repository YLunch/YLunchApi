using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace YLunchApi.Authentication.Exceptions;

[Serializable]
public class RefreshTokenExpiredException : Exception
{
    [ExcludeFromCodeCoverage]
    private RefreshTokenExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public RefreshTokenExpiredException()
    {
    }
}
