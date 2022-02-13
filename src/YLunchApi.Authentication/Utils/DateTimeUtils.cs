using System.Diagnostics.CodeAnalysis;

namespace YLunchApi.Authentication.Utils;

[ExcludeFromCodeCoverage]
public static class DateTimeUtils //NOSONAR
{
    public static DateTime ConvertUnixTimeStampToDateTime(long unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return dateTime.AddSeconds(unixTimeStamp);
    }
}
