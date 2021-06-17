namespace YnovEat.DomainShared.RestaurantAggregate.Enums
{
    public enum OrderState
    {
        Idling,
        Acknowledged,
        InPreparation,
        Ready,
        Canceled,
        Rejected,
        Delivered,
        Other
    }
}