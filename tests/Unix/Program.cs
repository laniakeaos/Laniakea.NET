using Laniakea.Unix;
using Laniakea.Unix.Identity;

namespace Laniakea.Tests.Unix;

public class Program
{
    private static void UserInfo()
    {
        UnixUserInfo userInfo = new UnixUserInfo("root");
        Console.WriteLine($"{userInfo.Name} - uid: {userInfo.Uid}, gid: {userInfo.Gid}");
    }

    public static void Main()
    {
        try {
            UtsName utsName = UtsName.Uname();

            Console.WriteLine("sysname:     " + utsName.Sysname);
            Console.WriteLine("nodename:    " + utsName.Nodename);
            Console.WriteLine("release:     " + utsName.Release);
            Console.WriteLine("version:     " + utsName.Version);
            Console.WriteLine("machine:     " + utsName.Machine);
        } catch (Exception e) {
            Console.WriteLine(e);
        }

        UserInfo();
    }
}
