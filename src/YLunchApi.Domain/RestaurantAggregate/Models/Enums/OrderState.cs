namespace YLunchApi.Domain.RestaurantAggregate.Models.Enums
{
    // !!! IMPORTANT: Update Count of OrderStateUtils when add value !!!
    public enum OrderState
    {
        // !!! IMPORTANT: Update Count of OrderStateUtils when add value !!!
        Idling,
        Acknowledged,
        InPreparation,
        Ready,
        Delivered,
        Other,
        Canceled,
        Rejected
    }

    public static class OrderStateUtils
    {
        // !!! IMPORTANT: Update Count of OrderStateUtils when add value !!!
        public const int Count = 8;
    }
}
