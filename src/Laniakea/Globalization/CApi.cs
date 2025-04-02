using System.Runtime.InteropServices;

namespace Laniakea.Globalization.CApi;

public static class CIntl
{
    internal const int LC_CTYPE = 0;
    internal const int LC_NUMERIC = 1;
    internal const int LC_TIME = 2;
    internal const int LC_COLLATE = 3;
    internal const int LC_MONETARY = 4;
    internal const int LC_MESSAGES = 5;
    internal const int LC_ALL = 6;

    [DllImport("libc", CharSet = CharSet.Ansi)]
    internal static extern string? setlocale(int category, string? locale);

    [DllImport("libc", CharSet = CharSet.Ansi)]
    internal static extern string? textdomain(string? domainname);

    [DllImport("libc", CharSet = CharSet.Ansi)]
    internal static extern string? bindtextdomain(string domainname, string? dirname);
}
