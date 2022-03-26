namespace YLunchApi.Domain.CommonAggregate.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
