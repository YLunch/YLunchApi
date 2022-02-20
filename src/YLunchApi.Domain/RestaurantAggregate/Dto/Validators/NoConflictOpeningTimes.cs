using System.ComponentModel.DataAnnotations;

namespace YLunchApi.Domain.RestaurantAggregate.Dto.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class NoConflictOpeningTimes : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return false;
        }

        var orderedOpeningTimes = ((ICollection<OpeningTimeCreateDto>)value)
                                  .OrderBy(x => x.Start)
                                  .ThenBy(x => x.End)
                                  .ToList();

        for (var i = 1; i < orderedOpeningTimes.Count; i++)
        {
            var previousOpeningTimes = orderedOpeningTimes[i - 1];
            var currentOpeningTimes = orderedOpeningTimes[i];

            if (previousOpeningTimes.End >= currentOpeningTimes.Start)
            {
                return false;
            }
        }

        return true;
    }

    public override string FormatErrorMessage(string name)
    {
        return "Some opening times override others";
    }
}
