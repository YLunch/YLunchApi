using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace YLunchApi.Authentication.Exceptions;

[Serializable]
public class RefreshTokenNotFoundException : Exception
{
    [ExcludeFromCodeCoverage]
    private RefreshTokenNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public RefreshTokenNotFoundException()
    {
    }
}
