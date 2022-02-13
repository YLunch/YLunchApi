namespace YLunchApi.Authentication.Utils;

public static class RandomUtils
{
    public static string GetRandomKey()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..22];
    }
}
