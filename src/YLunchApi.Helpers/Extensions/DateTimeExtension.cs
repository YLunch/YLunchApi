namespace YLunchApi.Helpers.Extensions;

public static class DateTimeExtension
{
    public static int MinutesFromMidnight(this DateTime dateTime)
    {
        return dateTime.Hour * 60 + dateTime.Minute;
    }
}
