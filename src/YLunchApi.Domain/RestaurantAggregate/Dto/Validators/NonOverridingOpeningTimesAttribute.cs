using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.Core.Utils;

namespace YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class NonOverridingOpeningTimesAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true;
        }

        var orderedOpeningTimes = ((ICollection<OpeningTimeCreateDto>)value)
                                  .OrderBy(x=>x.OffsetTime)
                                  .ThenBy(x=>x.OffsetTime!.Value.AddMinutes(x.DurationInMinutes!.Value))
                                  .ToList();

        for (var i = 1; i < orderedOpeningTimes.Count; i++)
        {
            var previousOpeningTime = orderedOpeningTimes[i - 1];
            var currentOpeningTime = orderedOpeningTimes[i];

            if (currentOpeningTime.OffsetTime!.Value <=previousOpeningTime.OffsetTime!.Value.AddMinutes(previousOpeningTime.DurationInMinutes!.Value))
            {
                return false;
            }
        }

        return true;
    }

    public override string FormatErrorMessage(string name)
    {
        return "Some opening times override others.";
    }
}
