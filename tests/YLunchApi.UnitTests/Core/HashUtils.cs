using System.Security.Cryptography;
using System.Text;

namespace YLunchApi.UnitTests.Core;

public static class HashUtils
{
    public static string HashValue(string value)
    {
        return MD5.HashData(Encoding.ASCII.GetBytes(value)).ToString()!;
    }
}
