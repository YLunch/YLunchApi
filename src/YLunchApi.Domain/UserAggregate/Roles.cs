namespace YLunchApi.Domain.UserAggregate;

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Customer = "Customer";
    public const string RestaurantAdmin = "RestaurantAdmin";
    private const char Separator = ',';

    public static IEnumerable<string> GetList()
    {
        return typeof(Roles).GetFields().Select(x => x.Name).ToList();
    }

    public static List<string> StringToList(string roles)
    {
        return roles.Split(Separator).ToList();
    }

    public static string ListToString(IEnumerable<string> roles)
    {
        return string.Join(Separator, roles);
    }
}
