using System.Runtime.InteropServices;
using Xdg.DBus.CApi;

namespace Xdg.DBus;

public class DBusException : Exception
{
    //
}

public class DBusMessage
{
    internal IntPtr _cPtr = IntPtr.Zero;

    public DBusMessageType Type { get; set; }
    public string? Destination { get; set; }
    public string Path { get; set; }
    public string? Interface { get; set; }
    public string Method { get; set; }
    
    public List<DBusArgument> Arguments { get; set; } = [];

    ~DBusMessage()
    {
        CDBus.dbus_message_unref(_cPtr);
    }

    public static DBusMessage CreateMethodCall(string? destination, string path, string? iface, string method)
    {
        var message = new DBusMessage();

        message.Type = DBusMessageType.MethodCall;
        message.Destination = destination;
        message.Path = path;
        message.Interface = iface;
        message.Method = method;

        message._cPtr = CDBus.dbus_message_new_method_call(message.Destination, message.Path, message.Interface, message.Method);

        return message;
    }
}

public class DBusConnection
{
    private DBusBusType _busType;
    private IntPtr _cPtr = IntPtr.Zero;

    public DBusBusType BusType
    {
        get => _busType;
    }

    public DBusConnection(DBusBusType busType)
    {
        _busType = busType;
        int cBusType = CDBus.DBUS_BUS_SESSION;
        switch (BusType)
        {
            case DBusBusType.Session:
            {
                cBusType = CDBus.DBUS_BUS_SESSION;
                break;
            }
            case DBusBusType.System:
            {
                cBusType = CDBus.DBUS_BUS_SYSTEM;
                break;
            }
            default:
            {
                // Error.
                break;
            }
        }

        CDBus.DBusError err = new CDBus.DBusError();
        IntPtr errPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CDBus.DBusError)));
        Marshal.StructureToPtr(err, errPtr, false);
        CDBus.dbus_error_init(errPtr);

        IntPtr connPtr = CDBus.dbus_bus_get(cBusType, errPtr);
        // Error check.
        if (CDBus.dbus_error_is_set(errPtr) == 1)
        {
            CDBus.dbus_error_free(errPtr);
            Marshal.FreeHGlobal(errPtr);
            throw new DBusException();
        }
        CDBus.dbus_error_free(errPtr);
        Marshal.FreeHGlobal(errPtr);
        _cPtr = connPtr;
    }

    public DBusMessage Send(DBusMessage message, int timeout = -1)
    {
        IntPtr argsIter = CDBus.la_dbus_message_iter_new(message._cPtr);
        foreach (var arg in message.Arguments)
        {
            switch (arg.Type)
            {
                case DBusType.Int32:
                    // CDBus.dbus_message_append_args(message._cPtr, [CDBus.DBUS_TYPE_INT32, (int)arg.Value]);
                    CDBus.la_dbus_message_append_int32_arg(message._cPtr, argsIter, (int)arg.Value);
                    break;
                case DBusType.UInt32:
                    CDBus.la_dbus_message_append_uint32_arg(message._cPtr, argsIter, (uint)arg.Value);
                    break;
                case DBusType.String:
                    // CDBus.dbus_message_append_args(message._cPtr, ]);
                    break;
                default:
                    break;
            }
        }

        // CDBus.la_dbus_message_finish_arg(message._cPtr);

        // Error init.
        CDBus.DBusError err = new CDBus.DBusError();
        IntPtr errPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CDBus.DBusError)));
        Marshal.StructureToPtr(err, errPtr, false);

        IntPtr cMsg = CDBus.dbus_connection_send_with_reply_and_block(_cPtr, message._cPtr, timeout, errPtr);
        // TODO. Error check.

        DBusMessage retMsg = new DBusMessage();

        return retMsg;
    }
}

public enum DBusType
{
    Byte,
    Boolean,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Double,
    UnixFD,
    String,
    ObjectPath,
    Signature,
    Array,
    Struct,
    Variant,
    DictEntry,
}

public enum DBusBusType
{
    Session,
    System,
}

public enum DBusMessageType
{
    MethodCall,
    MethodReturn,
    Error,
    Signal,
}

public class DBusVariant
{
    public DBusType Type { get; set; }
    public object Value { get; set; }

    public DBusVariant(byte value)
    {
        Type = DBusType.Byte;
        Value = value;
    }

    public DBusVariant(bool value)
    {
        Type = DBusType.Boolean;
        Value = value;
    }

    public DBusVariant(Int16 value)
    {
        Type = DBusType.Int16;
        Value = value;
    }

    public DBusVariant(UInt16 value)
    {
        Type = DBusType.UInt16;
        Value = value;
    }

    public DBusVariant(int value)
    {
        Type = DBusType.Int32;
        Value = value;
    }

    public DBusVariant(uint value)
    {
        Type = DBusType.UInt32;
        Value = value;
    }

    public DBusVariant(Int64 value)
    {
        Type = DBusType.Int64;
        Value = value;
    }

    public DBusVariant(UInt64 value)
    {
        Type = DBusType.UInt64;
        Value = value;
    }

    public DBusVariant(double value)
    {
        Type = DBusType.Double;
        Value = value;
    }

    public DBusVariant(string value)
    {
        Type = DBusType.String;
        Value = value;
    }
}

public class DBusDictEntry
{
    public DBusType KeyType { get; set; }
    public DBusType ValueType { get; set; }
    
    public object Key { get; set; }
    public object Value { get; set; }

    public bool IsValid
    {
        get
        {
            DBusType[] invalidList = [DBusType.Boolean, DBusType.Double, DBusType.Array, DBusType.Struct];
            return !invalidList.Contains(KeyType);
        }
    }
}

public class DBusArgument
{
    public DBusType Type { get; set; }
    
    public object Value { get; set; }

    public DBusArgument(byte value)
    {
        Type = DBusType.Byte;
        Value = value;
    }

    public DBusArgument(bool value)
    {
        Type = DBusType.Boolean;
        Value = value;
    }

    public DBusArgument(Int16 value)
    {
        Type = DBusType.Int16;
        Value = value;
    }

    public DBusArgument(UInt16 value)
    {
        Type = DBusType.UInt16;
        Value = value;
    }

    public DBusArgument(int value)
    {
        Type = DBusType.Int32;
        Value = value;
    }

    public DBusArgument(uint value)
    {
        Type = DBusType.UInt32;
        Value = value;
    }

    public DBusArgument(Int64 value)
    {
        Type = DBusType.Int64;
        Value = value;
    }

    public DBusArgument(UInt64 value)
    {
        Type = DBusType.UInt64;
        Value = value;
    }

    public DBusArgument(double value)
    {
        Type = DBusType.Double;
        Value = value;
    }

    public DBusArgument(string value)
    {
        Type = DBusType.String;
        Value = value;
    }

    public DBusArgument(DBusVariant value)
    {
        Type = DBusType.Variant;
        Value = value;
    }
}