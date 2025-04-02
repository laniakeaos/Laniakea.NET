using System;
using System.Globalization;

using Laniakea.Globalization.CApi;

namespace Laniakea.Globalization;

public enum LocaleCategory
{
    All,
    Collate,
    CType,
    Messages,
    Monetary,
    Numeric,
    Time,
}

public static class Locale
{
    /// <summary>
    /// Get UNIX locale string from the CultureInfo instance.
    /// </summary>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    private static string CultureInfoToLocale(CultureInfo cultureInfo)
    {
        string locale = cultureInfo.Name.Replace("-", "_") + ".UTF-8";

        return locale;
    }

    private static CultureInfo LocaleToCultureInfo(string locale)
    {
        string name = locale.Replace("_", "-").Replace(".UTF-8", "");

        return new CultureInfo(name);
    }

    public static string SetLocale(LocaleCategory category, string? locale)
    {
        string? ret = category switch
        {
            LocaleCategory.All => CIntl.setlocale(CIntl.LC_ALL, locale),
            LocaleCategory.Collate => CIntl.setlocale(CIntl.LC_COLLATE, locale),
            LocaleCategory.CType => CIntl.setlocale(CIntl.LC_CTYPE, locale),
            LocaleCategory.Messages => CIntl.setlocale(CIntl.LC_MESSAGES, locale),
            LocaleCategory.Monetary => CIntl.setlocale(CIntl.LC_MONETARY, locale),
            LocaleCategory.Numeric => CIntl.setlocale(CIntl.LC_NUMERIC, locale),
            LocaleCategory.Time => CIntl.setlocale(CIntl.LC_TIME, locale),
            _ => null
        };

        if (ret == null)
        {
            throw new Exception("Failed to set locale");
        }

        return ret;
    }
}

public static class TextDomain
{
    public static string DomainName
    {
        get
        {
            string? domainName = CIntl.textdomain(null);
            if (domainName == null)
            {
                return "";
            }

            return domainName;
        }

        set {
            CIntl.textdomain(value);
        }
    }

    public static string DirectoryName
    {
        get
        {
            string? dirName = CIntl.bindtextdomain(DomainName, null);
            if (dirName == null)
            {
                return "";
            }

            return dirName;
        }
    }

    public static void Bind()
    {
        CIntl.bindtextdomain(DomainName, null);
    }

    public static void Bind(string dirname)
    {
        CIntl.bindtextdomain(DomainName, dirname);
    }
}
