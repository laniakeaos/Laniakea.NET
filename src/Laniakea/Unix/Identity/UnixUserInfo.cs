using System.Runtime.InteropServices;
using Laniakea.Unix.CApi;

namespace Laniakea.Unix.Identity;

public class UnixUserInfo
{
    public UnixUserInfo(string username)
    {
        var passwd = CPwd.getpwnam(username);

        if (passwd == IntPtr.Zero)
        {
            throw new ArgumentException($"{username} not found");
        }

        var passwdStruct = Marshal.PtrToStructure<CPwd.passwd>(passwd);

        Name = username;
        Uid = passwdStruct.pw_uid;
        Gid = passwdStruct.pw_gid;
    }

    public string Name { get; }

    public uint Uid { get; }

    public uint Gid { get; }
}
