using System.Runtime.InteropServices;

namespace Laniakea.Xdg.DBus.CApi;

internal class CDBus
{
    private const string LibdbusSo = "libdbus-1.so";

    internal const int DBUS_BUS_SESSION = 0;
    internal const int DBUS_BUS_SYSTEM = 1;
    internal const int DBUS_BUS_STARTER = 2;

    #pragma warning disable CS0169
    internal struct DBusError
    {
        private int p1;
        private int p2;
        private int p3;
        private int p4;
    }

    internal struct DBusMessageIter
    {
        IntPtr dummy1;
        IntPtr dummy2;
        UInt32 dummy3;
        int dummy4;
        int dummy5;
        int dummy6;
        int dummy7;
        int dummy8;
        int dummy9;
        int dummy10;
        int dummy11;
        int pad1;
        IntPtr pad2;
        IntPtr pad3;
    };

    internal const int DBUS_TYPE_BYTE = (int)'y';
    internal const int DBUS_TYPE_BOOLEAN = (int)'b';
    internal const int DBUS_TYPE_INT16 = (int)'n';
    internal const int DBUS_TYPE_UINT16 = (int)'q';
    internal const int DBUS_TYPE_INT32 = (int)'i';
    internal const int DBUS_TYPE_UINT32 = (int)'u';
    internal const int DBUS_TYPE_INT64 = (int)'x';
    internal const int DBUS_TYPE_UINT64 = (int)'t';
    internal const int DBUS_TYPE_DOUBLE = (int)'d';
    internal const int DBUS_TYPE_STRING = (int)'s';
    internal const int DBUS_TYPE_ARRAY = (int)'a';
    internal const int DBUS_TYPE_VARIANT = (int)'v';
    internal const int DBUS_TYPE_STRUCT = (int)'r';
    internal const int DBUS_TYPE_DICT_ENTRY = (int)'e';
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
        [MarshalAs(UnmanagedType.LPStr)] string? destination,
        [MarshalAs(UnmanagedType.LPStr)] string path,
        [MarshalAs(UnmanagedType.LPStr)] string? iface,
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

    [DllImport(LibdbusSo, CharSet = CharSet.Ansi)]
    internal static extern IntPtr dbus_message_get_signature(IntPtr message);

    [DllImport(LibdbusSo)]
    internal static extern uint dbus_message_iter_init(IntPtr message, ref CDBus.DBusMessageIter iter);

    [DllImport(LibdbusSo)]
    internal static extern uint dbus_message_iter_init_append(IntPtr message, ref CDBus.DBusMessageIter iter);

    [DllImport(LibdbusSo)]
    internal static extern uint dbus_message_iter_has_next(ref CDBus.DBusMessageIter iter);

    [DllImport(LibdbusSo)]
    internal static extern uint dbus_message_iter_next(ref CDBus.DBusMessageIter iter);

    [DllImport(LibdbusSo)]
    internal static extern int dbus_message_iter_get_arg_type(ref CDBus.DBusMessageIter iter);

    [DllImport(LibdbusSo)]
    internal static extern void dbus_message_iter_get_basic(ref CDBus.DBusMessageIter iter, ref IntPtr value);

    [DllImport(LibdbusSo)]
    internal static extern int dbus_message_iter_append_basic(ref CDBus.DBusMessageIter iter, int type, IntPtr value);

    [DllImport(LibdbusSo)]
    internal static extern int dbus_message_iter_open_container(ref CDBus.DBusMessageIter iter, int type,
        [MarshalAs(UnmanagedType.LPStr)] string? containedSignature,
        ref CDBus.DBusMessageIter sub);

    [DllImport(LibdbusSo)]
    internal static extern int dbus_message_iter_close_container(ref CDBus.DBusMessageIter iter, ref CDBus.DBusMessageIter sub);
}
