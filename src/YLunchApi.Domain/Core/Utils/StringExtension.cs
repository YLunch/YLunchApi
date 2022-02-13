using System.Text.RegularExpressions;

namespace YLunchApi.Domain.Core.Utils;

public static class StringExtension
{
    public static string Capitalize(this string value)
    {
        var regex = new Regex(@"([^\W0-9]+)");
        var matches = regex.Matches(value);
        for (var i = 0; i < matches.Count; i++)
        {
            var part = matches[i].Value;
            value = value.Replace(part, CapitalizePart(part));
        }

        return value;
    }

    private static string CapitalizePart(string value)
    {
        return value.Length switch
        {
            1 => char.ToUpper(value[0]).ToString(),
            _ => char.ToUpper(value[0]) + value[1..].ToLower()
        };
    }
}
