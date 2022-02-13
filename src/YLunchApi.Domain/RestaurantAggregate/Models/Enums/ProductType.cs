namespace YLunchApi.Domain.RestaurantAggregate.Models.Enums
{
    // IMPORTANT: Update Count of ProductFamiliesUtils when add value !!!
    public enum ProductType
    {
        Starter,
        Main,
        Dessert,
        Drink,
        Menu,
        Daily,
        Other
    }

    public static class ProductFamilyUtils
    {
        public const int Count = 7;
    }
}
