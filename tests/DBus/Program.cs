using Laniakea.Xdg.DBus;

namespace Laniakea.Tests.Xdg.DBus;

public class Program
{
    public static void Main()
    {
        try {
            DBusConnection conn = new DBusConnection(DBusBusType.Session);

            DBusMessage msg = DBusMessage.CreateMethodCall(
                "org.freedesktop.Notifications",
                "/org/freedesktop/Notifications",
                "org.freedesktop.Notifications",
                "CloseNotification"
            );
            var arg = new DBusArgument((uint)8);
            if (arg.Type != DBusType.UInt32) {
                Console.WriteLine("Not UInt32 value!");
            } else {
                Console.WriteLine(arg.Value);
            }
            msg.Arguments.Add(arg);

            conn.Send(msg);
        } catch (DBusException e) {
            Console.WriteLine("Error!");
        }
    }
}
