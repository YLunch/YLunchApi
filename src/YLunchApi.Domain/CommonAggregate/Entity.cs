using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.Core.Utils;

namespace YLunchApi.Domain.CommonAggregate;

public class Entity
{
    [Required] public string Id { get; set; } = Guid.NewGuid().ToString();
}
