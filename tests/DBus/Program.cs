using Xdg.DBus;

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
                "Notify"
            );

            conn.Send(msg);
        } catch (DBusException e) {
            Console.WriteLine("Error!");
        }
    }
}
