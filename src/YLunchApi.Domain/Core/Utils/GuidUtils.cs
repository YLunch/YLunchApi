namespace YLunchApi.Domain.Core.Utils;

public static class GuidUtils
{
    public const string Regex = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";
}
