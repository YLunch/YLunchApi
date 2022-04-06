using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace YLunchApi.Domain.Exceptions;

[Serializable]
public sealed class ReservedForDateTimeOutOfOpenToOrderOpeningTimesException : Exception
{
    public ReservedForDateTimeOutOfOpenToOrderOpeningTimesException(string message = "") : base(message)
    {
    }

    [ExcludeFromCodeCoverage]
    private ReservedForDateTimeOutOfOpenToOrderOpeningTimesException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
