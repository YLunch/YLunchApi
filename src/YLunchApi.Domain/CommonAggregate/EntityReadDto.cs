using YLunchApi.Domain.Core.Utils;

namespace YLunchApi.Domain.CommonAggregate;

public class EntityReadDto
{
    public string Link { get; set; } = null!;
    public string Id { get; set; } = null!;

    public EntityReadDto()
    {
    }

    protected EntityReadDto(string id, string resourcesName)
    {
        Id = id;
        Link = $"{EnvironmentUtils.BaseUrl}/{resourcesName}/{id}";
    }
}
