using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace YLunchApi.Authentication.Exceptions;

[Serializable]
public sealed class AccessAndRefreshTokensNotMatchException : Exception
{
    [ExcludeFromCodeCoverage]
    private AccessAndRefreshTokensNotMatchException(SerializationInfo info, StreamingContext context) : base(info,
        context)
    {
    }

    public AccessAndRefreshTokensNotMatchException()
    {
    }
}
