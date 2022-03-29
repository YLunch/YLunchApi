using YLunchApi.Domain.RestaurantAggregate.Dto;
using YLunchApi.Domain.RestaurantAggregate.Models.Enums;

namespace YLunchApi.TestsShared.Mocks;

public static class ProductMocks
{
    public static ProductCreateDto ProductCreateDto => new()
    {
        Name = "margherite",
        Price = 9.80,
        Description = "tomate, fromage",
        IsActive = true,
        Quantity = 3,
        ProductType = ProductType.Main,
        ExpirationDateTime = DateTimeMocks.Monday20220321T1000Utc.AddDays(1),
        Allergens = new List<AllergenCreateDto>
        {
            new() { Name = "gluten" },
            new() { Name = "arachide" }
        },
        ProductTags = new List<ProductTagCreateDto>
        {
            new() { Name = "pizza" },
            new() { Name = "italienne" }
        },
        Image = "data:image/png;base64,iVBORw0KGgoAAA..."
    };
}
