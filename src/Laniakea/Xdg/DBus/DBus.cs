using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections;

using Laniakea.Xdg.DBus.CApi;

namespace Laniakea.Xdg.DBus;

public class DBusException : Exception
{
    public DBusException() { }

    public DBusException(string message) : base(message)
    {
    }
}

internal class DBusInternalException : Exception
{
    public DBusInternalException() { }

    public DBusInternalException(string message) : base(message)
    {
    }
}

public class DBusMessage
{
    internal IntPtr _cPtr = IntPtr.Zero;

    public DBusMessageType Type { get; set; }
    public string? Destination { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? Interface { get; set; }
    public string Method { get; set; } = string.Empty;

    public List<DBusArgument> Arguments { get; set; } = [];
    public DBusSignature Signature { get; set; } = string.Empty;

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

    public DBusBusType BusType => _busType;

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
        DBusMessageIter argsIter = DBusMessageIter.InitAppend(message._cPtr);
        foreach (var arg in message.Arguments)
        {
            if (arg.Type != DBusType.Array && arg.Type != DBusType.Variant && arg.Type != DBusType.Struct && arg.Type != DBusType.DictEntry)
            {
                argsIter.AppendBasic(arg);
            }
        }

        // Error init.
        CDBus.DBusError err = new CDBus.DBusError();
        IntPtr errPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CDBus.DBusError)));
        Marshal.StructureToPtr(err, errPtr, false);

        IntPtr cMsg = CDBus.dbus_connection_send_with_reply_and_block(_cPtr, message._cPtr, timeout, errPtr);
        // TODO. Error check.

        DBusMessage retMsg = new DBusMessage();
        retMsg.Type = DBusMessageType.MethodReturn;

        // Get and set the signature.
        IntPtr sig = CDBus.dbus_message_get_signature(cMsg);
        string? signature = Marshal.PtrToStringAnsi(sig);
        if (signature != null)
        {
            retMsg.Signature = signature;
            DBusMessageIter iter = DBusMessageIter.Init(cMsg);
            var args = iter.ToArgumentList();
            retMsg.Arguments = args;
        }
        else
        {
            retMsg.Signature = string.Empty;
        }

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

public class DBusArray
{
    private DBusType _type;
    private List<object> _values = [];
    private DBusSignature _typeSignature;

    private void ValidateType(object value)
    {
        switch (_type)
        {
            case DBusType.Byte when value.GetType() != typeof(byte):
            case DBusType.Boolean when value.GetType() != typeof(bool):
            case DBusType.Int16 when value.GetType() != typeof(Int16):
            case DBusType.UInt16 when value.GetType() != typeof(UInt16):
            case DBusType.Int32 when value.GetType() != typeof(Int32):
            case DBusType.UInt32 when value.GetType() != typeof(UInt32):
            case DBusType.Int64 when value.GetType() != typeof(Int64):
            case DBusType.UInt64 when value.GetType() != typeof(UInt64):
            case DBusType.Double when value.GetType() != typeof(Double):
            case DBusType.String when value.GetType() != typeof(string):
            case DBusType.Array when value.GetType() != typeof(DBusArray):
            case DBusType.Variant when value.GetType() != typeof(DBusVariant):
            // TODO: case DBusType.Struct when value.GetType() != typeof(DBusStruct):
            case DBusType.DictEntry when value.GetType() != typeof(DBusDictEntry):
                throw new ArgumentException("Type is mismatch.");
            default:
                return;
        }
    }

    public DBusArray(DBusType type)
    {
        _type = type;
    }

    public void Add(byte value)
    {
        ValidateType(value);
        _values.Add(value);
    }

    public void Add(bool value)
    {
        ValidateType(value);
        _values.Add(value);
    }

    public void Add(Int16 value)
    {
        ValidateType(value);
        _values.Add(value);
    }

    public void Add(UInt16 value)
    {
        ValidateType(value);
        _values.Add(value);
    }

    public void Add(int value)
    {
        ValidateType(value);
        _values.Add(value);
    }

    public void Add(uint value)
    {
        ValidateType(value);
        _values.Add(value);
    }

    public void Add(Int64 value)
    {
        ValidateType(value);
        _values.Add(value);
    }

    public void Add(UInt64 value)
    {
        ValidateType(value);
        _values.Add(value);
    }

    public void Add(double value)
    {
        ValidateType(value);
        _values.Add(value);
    }

    public void Add(string value)
    {
        ValidateType(value);
        _values.Add(value);
    }
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

    /// <summary>
    /// Get the signature of the variant type.
    /// </summary>
    public DBusSignature TypeSignature
    {
        get
        {
            return DBusSignature.TypeToSignature(Type);
        }
    }
}

public class DBusDictEntry
{
    private object _key = string.Empty;
    private object _value = string.Empty;

    public DBusType KeyType { get; set; }
    public DBusType ValueType { get; set; }

    public object Key
    {
        get => _key;
        set
        {
            switch (value)
            {
                case byte b:
                    _key = b;
                    KeyType = DBusType.Byte;
                    break;
                case bool b:
                    _key = b;
                    KeyType = DBusType.Boolean;
                    break;
                case Int16 i16:
                    _key = i16;
                    KeyType = DBusType.Int16;
                    break;
                case UInt16 u16:
                    _key = u16;
                    KeyType = DBusType.UInt16;
                    break;
                case int i32:
                    _key = i32;
                    KeyType = DBusType.Int32;
                    break;
                case uint u32:
                    _key = u32;
                    KeyType = DBusType.UInt32;
                    break;
                case Int64 i64:
                    _key = i64;
                    KeyType = DBusType.Int64;
                    break;
                case UInt64 u64:
                    _key = u64;
                    KeyType = DBusType.UInt64;
                    break;
                case double d:
                    _key = d;
                    KeyType = DBusType.Double;
                    break;
                case string s:
                    _key = s;
                    KeyType = DBusType.String;
                    break;
                case DBusVariant v:
                    _key = v;
                    KeyType = DBusType.Variant;
                    break;
            }
        }
    }

    public object Value { get; set; }

    public DBusDictEntry()
    {
        KeyType = DBusType.String;
        ValueType = DBusType.Variant;
        Key = string.Empty;
        Value = new DBusVariant(string.Empty);
    }

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
    public DBusType Type { get; private set; }

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

    public DBusSignature Signature
    {
        get
        {
            return DBusSignature.TypeToSignature(Type);
        }
    }
}
