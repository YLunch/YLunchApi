using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace YLunchApi.Domain.Exceptions;

[Serializable]
public sealed class SoldOutProductsException : Exception
{
    public SoldOutProductsException(string message = "") : base(message)
    {
    }

    [ExcludeFromCodeCoverage]
    private SoldOutProductsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
