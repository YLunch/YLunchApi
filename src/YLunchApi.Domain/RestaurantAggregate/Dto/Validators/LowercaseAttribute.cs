using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class LowercaseAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true;
        }

        return value.ToString() == value.ToString()?.ToLower();
    }

    public override string FormatErrorMessage(string name)
    {
        return "Must be lowercase.";
    }
}
