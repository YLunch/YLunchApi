using YLunchApi.Domain.Core.Utils;

namespace YLunchApi.Domain.CommonAggregate.Dto;

public abstract class EntityReadDto
{
    public string Id { get; set; } = null!;

    public EntityReadDto()
    {
    }
}
