namespace Laniakea.Unix;

using System;
using System.Runtime.InteropServices;

public class UtsName
{
    public string? Sysname { get; private set; }
    public string? Nodename { get; private set; }
    public string? Release { get; private set; }
    public string? Version { get; private set; }
    public string? Machine { get; private set; }

    static public UtsName Uname()
    {
        UtsName utsName = new UtsName();

        CStructUtsname cUtsName = new CStructUtsname();
        int result = uname(ref cUtsName);

        if (result == -1) {
            throw new InvalidOperationException("Failed to get uname!");
        }

        utsName.Sysname = cUtsName.sysname;
        utsName.Nodename = cUtsName.nodename;
        utsName.Release = cUtsName.release;
        utsName.Version = cUtsName.version;
        utsName.Machine = cUtsName.machine;

        return utsName;
    }

    internal const int _UTSNAME_LENGTH = 65;

    internal struct CStructUtsname
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _UTSNAME_LENGTH)]
        internal string sysname;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _UTSNAME_LENGTH)]
        internal string nodename;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _UTSNAME_LENGTH)]
        internal string release;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _UTSNAME_LENGTH)]
        internal string version;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _UTSNAME_LENGTH)]
        internal string machine;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _UTSNAME_LENGTH)]
        internal string domainname;
    }

    [DllImport("libc")]
    internal static extern int uname(ref CStructUtsname name);
}
