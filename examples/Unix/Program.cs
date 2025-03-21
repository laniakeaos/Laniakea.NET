using Laniakea.Unix;

namespace Laniakea.Tests.Unix;

public class Program
{
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
    }
}
