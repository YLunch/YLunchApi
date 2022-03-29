using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace YLunchApi.Domain.CommonAggregate.Models;

public class Entity
{
    [Required] public string Id { get; [ExcludeFromCodeCoverage] set; } = Guid.NewGuid().ToString();
}
