using System.Runtime.InteropServices;

namespace Xdg.DBus.CApi;

internal class CDBus
{
    private const string LibdbusSo = "libdbus-1.so";
    private const string LibXdgDBusSo = "libXdg.DBus.so";

    internal const int DBUS_BUS_SESSION = 0;
    internal const int DBUS_BUS_SYSTEM = 1;
    internal const int DBUS_BUS_STARTER = 2;

    internal struct DBusError
    {
        private int p1;
        private int p2;
        private int p3;
        private int p4;
    }

    internal const int DBUS_TYPE_INT32 = 0x69;    // (int)'i'
    internal const int DBUS_TYPE_INVALID = 0x00;

    [DllImport(LibdbusSo)]
    internal static extern IntPtr dbus_bus_get(int type, IntPtr error);

    [DllImport(LibdbusSo)]
    internal static extern void dbus_error_init(IntPtr error);

    [DllImport(LibdbusSo)]
    internal static extern void dbus_error_free(IntPtr error);

    [DllImport(LibdbusSo)]
    internal static extern uint dbus_error_is_set(IntPtr error);

    [DllImport(LibdbusSo)]
    internal static extern IntPtr dbus_message_new_method_call(
        [MarshalAs(UnmanagedType.LPStr)] string destination,
        [MarshalAs(UnmanagedType.LPStr)] string path,
        [MarshalAs(UnmanagedType.LPStr)] string iface,
        [MarshalAs(UnmanagedType.LPStr)] string method);

    [DllImport(LibdbusSo)]
    internal static extern void dbus_message_unref(IntPtr message);

    [DllImport(LibdbusSo)]
    internal static extern void dbus_message_append_args(IntPtr message, params int[] values);

    [DllImport(LibdbusSo)]
    internal static extern uint dbus_connection_send(IntPtr conn, IntPtr message, uint serial);

    [DllImport(LibdbusSo)]
    internal static extern IntPtr dbus_connection_send_with_reply_and_block(IntPtr conn, IntPtr message, int timeout,
        IntPtr err);

    [DllImport(LibdbusSo)]
    internal static extern uint dbus_message_iter_append_basic(IntPtr iter, int type, [MarshalAs(UnmanagedType.LPStr)]string value);

    [DllImport(LibXdgDBusSo)]
    internal static extern uint la_dbus_message_finish_arg(IntPtr message);
}
