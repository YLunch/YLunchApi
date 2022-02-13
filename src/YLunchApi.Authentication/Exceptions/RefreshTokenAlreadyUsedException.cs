using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace YLunchApi.Authentication.Exceptions;

[Serializable]
public sealed class RefreshTokenAlreadyUsedException : Exception
{
    [ExcludeFromCodeCoverage]
    private RefreshTokenAlreadyUsedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public RefreshTokenAlreadyUsedException()
    {
    }
}
