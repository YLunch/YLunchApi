using System.ComponentModel.DataAnnotations;
using YLunchApi.Domain.Core.Utils;

namespace YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class NoOverridingOpeningTimesAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return false;
        }

        var orderedOpeningTimes = ((ICollection<OpeningTimeCreateDto>)value)
                                  .OrderBy(OpeningTimeUtils.StartMinutesInWeek)
                                  .ThenBy(OpeningTimeUtils.EndMinutesInWeek)
                                  .ToList();

        for (var i = 1; i < orderedOpeningTimes.Count; i++)
        {
            var previousOpeningTimes = orderedOpeningTimes[i - 1];
            var currentOpeningTimes = orderedOpeningTimes[i];

            if (OpeningTimeUtils.StartMinutesInWeek(currentOpeningTimes) <=
                OpeningTimeUtils.EndMinutesInWeek(previousOpeningTimes))
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
