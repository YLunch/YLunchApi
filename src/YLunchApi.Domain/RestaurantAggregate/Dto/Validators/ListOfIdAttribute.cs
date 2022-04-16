using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using YLunchApi.Domain.Core.Utils;

namespace YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ListOfIdAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value switch
        {
            null => true,
            ICollection<string> { Count: 0 } => false,
            ICollection<string> collection => collection.All(x => new Regex(GuidUtils.Regex).IsMatch(x)),
            _ => false
        };
    }

    public override string FormatErrorMessage(string name)
    {
        return "Must be a list of id which match Guid regular expression.";
    }
}
