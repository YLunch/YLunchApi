using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.CommonAggregate.Services;

namespace YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InFutureOrNullAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var dateTimeProvider = validationContext.GetService(typeof(IDateTimeProvider))! as IDateTimeProvider;
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if ((DateTime)value > dateTimeProvider!.UtcNow)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(FormatErrorMessage("DateTime"));
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} should be in future if present.";
    }
}
