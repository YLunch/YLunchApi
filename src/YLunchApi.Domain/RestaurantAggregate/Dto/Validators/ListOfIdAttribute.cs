using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using YLunchApi.Domain.Core.Utils;

namespace YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ListOfIdAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true;
        }

        if (value is List<string> list)
        {
            return list.TrueForAll(x => new Regex(GuidUtils.Regex).IsMatch(x));
        }

        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return "Must be a list of id which match Guid regular expression.";
    }
}
