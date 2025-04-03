using System.Runtime.InteropServices;

namespace Laniakea.Unix.CApi;

internal static class CUnistd
{
    [DllImport("libc", SetLastError = true)]
    internal static extern int chown(string pathname, uint owner, uint group);
}

internal static class CPwd
{
#pragma warning disable CS8981
    internal struct passwd
    {
        public string pw_name;
        public string pw_passwd;
        public uint pw_uid;
        public uint pw_gid;
        public string pw_gecos;
        public string pw_dir;
        public string pw_shell;
    }
#pragma warning restore CS8981

    [DllImport("libc", SetLastError = true)]
    internal static extern IntPtr getpwnam(string name);
}

internal static class CGrp
{
#pragma warning disable CS8981
    internal struct group
    {
        public IntPtr gr_name;
        public IntPtr gr_passwd;
        public uint gr_gid;
        public IntPtr gr_mem;
    }
#pragma warning restore CS8981

    [DllImport("libc", SetLastError = true)]
    internal static extern int getgrnam(string name);
}
