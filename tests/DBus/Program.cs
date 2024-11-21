using Laniakea.Xdg.DBus;

namespace Laniakea.Tests.Xdg.DBus;

public class Program
{
    private static void Signature()
    {
        string sigStr = "ia(ii){sv}a(i{sb})";
        DBusSignature signature = new DBusSignature(sigStr);

        Console.WriteLine("Signature string: " + signature);
        Console.WriteLine("Count: 4 == " + signature.Count);
        foreach (DBusSignature sig in signature)
        {
            Console.WriteLine(" - " + sig);
        }

        Console.WriteLine("2nd: a(ii) == " + signature[1]);
        DBusSignature arrayType = signature[1].ArrayType;
        Console.WriteLine("(ii) == " + arrayType);
    }

    private static void Introspect()
    {
        try
        {
            DBusConnection conn = new DBusConnection(DBusBusType.Session);

            DBusMessage msg = DBusMessage.CreateMethodCall(
                "org.freedesktop.Notifications",
                "/",
                "org.freedesktop.DBus.Introspectable",
                "Introspect"
            );
            DBusMessage retMsg = conn.Send(msg);

            Console.WriteLine(retMsg.Signature);
            if (retMsg.Arguments.Count > 0) {
                var arg = retMsg.Arguments[0];
                var xmlStr = (string)arg.Value;
                Console.WriteLine(xmlStr);

                DBusIntrospection introspection = new DBusIntrospection(xmlStr);
                Console.WriteLine(introspection);

                foreach (DBusObject obj in introspection.Objects)
                {
                    Console.WriteLine(obj);
                }
            }
        }
        catch (DBusException e)
        {
            Console.WriteLine("Error!");
        }
    }

    public static void Main()
    {
        Signature();

        Introspect();

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
