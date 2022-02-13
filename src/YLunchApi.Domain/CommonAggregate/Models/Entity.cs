using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.CommonAggregate.Models;

public class Entity
{
    [Required] public string Id { get; set; } = Guid.NewGuid().ToString();
}
