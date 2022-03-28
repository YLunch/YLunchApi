using YLunchApi.Domain.RestaurantAggregate.Dto;

namespace YLunchApi.TestsShared.Mocks;

public static class ProductMocks
{
    public static ProductCreateDto ProductCreateDto => new()
    {
        Name = "Margherite",
        Price = 9.80,
        Description = "Tomate, Fromage, Olives",
        IsActive = true,
        Quantity = 3,
        ExpirationDateTime = DateTimeMocks.Monday20220321T1000Utc.AddDays(1),
        Allergens = new List<AllergenCreateDto>
        {
            new() { Name = "gluten" },
            new() { Name = "arachide" }
        }
    };
}
