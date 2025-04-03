using System.Runtime.InteropServices;

namespace Laniakea.Unix.CApi;

internal static class CUnistd
{
    [DllImport("libc", SetLastError = true)]
    internal static extern int chown(string pathname, uint owner, uint group);
}

internal static class CPwd
{
    struct passwd
    {
        string pw_name;
        string pw_passwd;
        uint pw_uid;
        uint pw_gid;
        string pw_gecos;
        string pw_dir;
        string pw_shell;
    }

    [DllImport("libc", SetLastError = true)]
    internal static extern IntPtr getpwnam(string name);
}

internal static class CGrp
{
    public struct group
    {
        public IntPtr gr_name;
        public IntPtr gr_passwd;
        public uint gr_gid;
        public IntPtr gr_mem;
    }

    [DllImport("libc", SetLastError = true)]
    internal static extern int getgrnam(string name);
}
