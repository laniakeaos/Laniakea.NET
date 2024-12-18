using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections;
using System.Security.Cryptography;
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

internal class DBusMessageIter
{
    private IntPtr _cIter = IntPtr.Zero;
    private DBusMessageIter? _subIter = null;
    private IntPtr _cMessage = IntPtr.Zero;

    public DBusMessageIter()
    {
        _cIter = CDBus.la_dbus_message_iter_new();
    }

    ~DBusMessageIter()
    {
        CDBus.la_dbus_message_iter_free(_cIter);
    }

    public static DBusMessageIter InitAppend(IntPtr message)
    {
        DBusMessageIter iter = new DBusMessageIter();
        iter._cMessage = message;
        CDBus.dbus_message_iter_init_append(message, iter._cIter);

        return iter;
    }

    public static DBusMessageIter Init(IntPtr message)
    {
        DBusMessageIter iter = new DBusMessageIter();
        iter._cMessage = message;
        CDBus.dbus_message_iter_init(message, iter._cIter);

        return iter;
    }

    public DBusMessageIter? SubIter
    {
        get => _subIter;
    }

    public bool AppendBasic(DBusArgument arg)
    {
        IntPtr cPtr = IntPtr.Zero;
        int result = 0;

        switch (arg.Type)
        {
            case DBusType.Byte:
                cPtr = Marshal.AllocHGlobal(sizeof(byte));
                Marshal.WriteByte(cPtr, (byte)arg.Value);
                result = CDBus.dbus_message_iter_append_basic(_cIter, CDBus.DBUS_TYPE_BYTE, cPtr);
                break;
            case DBusType.Boolean:
                cPtr = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(cPtr, (bool)arg.Value == true ? 1 : 0);
                result = CDBus.dbus_message_iter_append_basic(_cIter, CDBus.DBUS_TYPE_BOOLEAN, cPtr);
                break;
            case DBusType.Int16:
                cPtr = Marshal.AllocHGlobal(sizeof(Int16));
                Marshal.WriteInt16(cPtr, (Int16)arg.Value);
                result = CDBus.dbus_message_iter_append_basic(_cIter, CDBus.DBUS_TYPE_INT16, cPtr);
                break;
            case DBusType.UInt16:
                cPtr = Marshal.AllocHGlobal(sizeof(UInt16));
                Marshal.WriteInt16(cPtr, unchecked((Int16)((UInt16)arg.Value)));
                result = CDBus.dbus_message_iter_append_basic(_cIter, CDBus.DBUS_TYPE_UINT16, cPtr);
                break;
            case DBusType.Int32:
                cPtr = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(cPtr, (int)arg.Value);
                result = CDBus.dbus_message_iter_append_basic(_cIter, CDBus.DBUS_TYPE_INT32, cPtr);
                break;
            case DBusType.UInt32:
                cPtr = Marshal.AllocHGlobal(sizeof(uint));
                Marshal.WriteInt32(cPtr, unchecked((int)((uint)arg.Value)));
                result = CDBus.dbus_message_iter_append_basic(_cIter, CDBus.DBUS_TYPE_UINT32, cPtr);
                break;
            case DBusType.Int64:
                cPtr = Marshal.AllocHGlobal(sizeof(Int64));
                Marshal.WriteInt64(cPtr, (Int64)arg.Value);
                result = CDBus.dbus_message_iter_append_basic(_cIter, CDBus.DBUS_TYPE_INT64, cPtr);
                break;
            case DBusType.UInt64:
                cPtr = Marshal.AllocHGlobal(sizeof(UInt64));
                Marshal.WriteInt64(cPtr, unchecked((Int64)((UInt64)arg.Value)));
                result = CDBus.dbus_message_iter_append_basic(_cIter, CDBus.DBUS_TYPE_UINT64, cPtr);
                break;
            case DBusType.String:
                cPtr = Marshal.StringToHGlobalAnsi((string)arg.Value);
                result = CDBus.dbus_message_iter_append_basic(_cIter, CDBus.DBUS_TYPE_STRING, cPtr);
                break;
        }

        if (cPtr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(cPtr);
        }

        return result == 1 ? true : false;
    }

    public bool OpenContainer(DBusType type, DBusSignature? signature)
    {
        _subIter = new DBusMessageIter();
        int cType = type switch
        {
            DBusType.Array => CDBus.DBUS_TYPE_ARRAY,
            DBusType.Variant => CDBus.DBUS_TYPE_VARIANT,
            DBusType.Struct => CDBus.DBUS_TYPE_STRUCT,
            DBusType.DictEntry => CDBus.DBUS_TYPE_DICT_ENTRY,
            _ => throw new DBusInternalException("Unknown type."),
        };

        int result = CDBus.dbus_message_iter_open_container(_cIter, cType, signature?.ToString(), _subIter._cIter);

        return result == 1 ? true : false;
    }

    public bool CloseContainer()
    {
        if (_subIter == null)
        {
            throw new DBusInternalException("Can't close container because there is no sub-iter.");
        }
        int result = CDBus.dbus_message_iter_close_container(_cIter, _subIter._cIter);

        return result == 1 ? true : false;
    }

    public List<DBusArgument> ToArgumentList()
    {
        List<DBusArgument> list = [];

        do
        {
            IntPtr cPtr = IntPtr.Zero;

            int argType = CDBus.dbus_message_iter_get_arg_type(_cIter);
            switch (argType)
            {
                case CDBus.DBUS_TYPE_BYTE:
                    CDBus.dbus_message_iter_get_basic(_cIter, ref cPtr);
                    byte val = Marshal.PtrToStructure<byte>(cPtr);
                    list.Add(new DBusArgument(val));
                    break;
                case CDBus.DBUS_TYPE_BOOLEAN:
                    CDBus.dbus_message_iter_get_basic(_cIter, ref cPtr);
                    int boolVal = Marshal.ReadInt32(cPtr);
                    list.Add(new DBusArgument(boolVal == 1 ? true : false));
                    break;
                case CDBus.DBUS_TYPE_INT16:
                    CDBus.dbus_message_iter_get_basic(_cIter, ref cPtr);
                    Int16 i16Val = Marshal.ReadInt16(cPtr);
                    list.Add(new DBusArgument(i16Val));
                    break;
                case CDBus.DBUS_TYPE_UINT16:
                    CDBus.dbus_message_iter_get_basic(_cIter, ref cPtr);
                    UInt16 u16Val = unchecked((UInt16)Marshal.ReadInt16(cPtr));
                    list.Add(new DBusArgument(u16Val));
                    break;
                case CDBus.DBUS_TYPE_INT32:
                    CDBus.dbus_message_iter_get_basic(_cIter, ref cPtr);
                    int i32Val = Marshal.ReadInt32(cPtr);
                    list.Add(new DBusArgument(i32Val));
                    break;
                case CDBus.DBUS_TYPE_UINT32:
                    CDBus.dbus_message_iter_get_basic(_cIter, ref cPtr);
                    uint u32Val = unchecked((uint)Marshal.ReadInt32(cPtr));
                    list.Add(new DBusArgument(u32Val));
                    break;
                case CDBus.DBUS_TYPE_INT64:
                    CDBus.dbus_message_iter_get_basic(_cIter, ref cPtr);
                    Int64 i64Val = Marshal.ReadInt64(cPtr);
                    list.Add(new DBusArgument(i64Val));
                    break;
                case CDBus.DBUS_TYPE_UINT64:
                    CDBus.dbus_message_iter_get_basic(_cIter, ref cPtr);
                    UInt64 u64Val = unchecked((UInt64)Marshal.ReadInt64(cPtr));
                    list.Add(new DBusArgument(u64Val));
                    break;
                case CDBus.DBUS_TYPE_STRING:
                    CDBus.dbus_message_iter_get_basic(_cIter, ref cPtr);
                    string str = Marshal.PtrToStringAnsi(cPtr)!;
                    list.Add(new DBusArgument(str));
                    break;
                default:
                    break;
            }
            CDBus.dbus_message_iter_next(_cIter);
        } while (CDBus.dbus_message_iter_has_next(_cIter) == 1);

        return list;
    }
}

public class DBusSignature : IEnumerable<DBusSignature>
{
    private string _string;
    private List<string> _signatures;

    /// <summary>
    /// Parses the container type within a DBus signature string.
    /// </summary>
    /// <param name="signature">The DBus signature string to parse.</param>
    /// <returns>
    /// A substring representing the entire container, starting with a container
    /// type ('{' or '(') and ending with its corresponding closing character ('}' or ')').
    /// </returns>
    /// <remarks>
    /// This method assumes the input string starts with a container type.
    /// If the signature string does not begin with a valid container, the behavior is undefined.
    /// Example:
    /// - Input: "{sv}ii" → Output: "{sv}"
    /// - Input: "(ii)" → Output: "(ii)"
    /// </remarks>
    private static string ParseContainer(string signature)
    {
        if (signature.StartsWith("{"))
        {
            string str = "{";
            int openCount = 0;
            signature = signature.Substring(1);
            while (!signature.StartsWith("}"))
            {
                if (signature.StartsWith("{"))
                {
                    openCount++;
                }
                str += signature.Substring(0, 1);
                signature = signature.Substring(1);
            }
            while (openCount > 0)
            {
                str += signature.Substring(0, 1);
                if (signature.StartsWith("}"))
                {
                    openCount--;
                }
                signature = signature.Substring(1);
            }
            str += "}";

            return str;
        } else if (signature.StartsWith("("))
        {
            string str = "(";
            int openCount = 0;
            signature = signature.Substring(1);
            while (!signature.StartsWith(")"))
            {
                if (signature.StartsWith("("))
                {
                    openCount++;
                }
                str += signature.Substring(0, 1);
                signature = signature.Substring(1);
            }
            while (openCount > 0)
            {
                str += signature.Substring(0, 1);
                if (signature.StartsWith(")"))
                {
                    openCount--;
                }
                signature = signature.Substring(1);
            }
            str += ")";

            return str;
        }

        return "";
    }

    public DBusSignature(string str)
    {
        _string = str;
        _signatures = new List<string>();

        string[] singleTypes = ["y", "b", "n", "q", "i", "u", "x", "t", "d", "h", "s", "o", "g", "v"];
        while (str != "")
        {
            string start = str.Substring(0, 1);
            if (singleTypes.Contains(start))
            {
                str = str.Substring(1);
                _signatures.Add(start);
            } else if (start == "a")
            {
                string singlePattern = "^a[a]*[b-z]+";
                string arrStr = "";
                if (Regex.IsMatch(str, singlePattern))
                {
                    arrStr = Regex.Match(str, singlePattern).Value;
                    str = str.Substring(arrStr.Length);
                }
                else
                {
                    while (str.StartsWith("a"))
                    {
                        arrStr += "a";
                        str = str.Substring(1);
                    }

                    string cont = ParseContainer(str);
                    arrStr += cont;
                    str = str.Substring(cont.Length);
                }

                _signatures.Add(arrStr);
            } else if (start == "(")
            {
                string structStr = ParseContainer(str);
                str = str.Substring(structStr.Length);
                _signatures.Add(structStr);
            } else if (start == "{")
            {
                string dictStr = ParseContainer(str);
                str = str.Substring(dictStr.Length);
                _signatures.Add(dictStr);
            }
        }
    }

    public DBusSignature ArrayType
    {
        get
        {
            if (IsArray == false)
            {
                return "";
            }

            string sigStr = _signatures[0].ToString();
            return new DBusSignature(sigStr.Substring(1));
        }
    }

    public DBusSignature InnerSignature
    {
        get
        {
            if (IsStruct == false)
            {
                return "";
            }

            string innerStr = _string.Substring(1, _string.Length - 2);
            return new DBusSignature(innerStr);
        }
    }

    public DBusSignature DictEntryKey
    {
        get
        {
            if (IsDictEntry == false)
            {
                return "";
            }

            string innerStr = _string.Substring(1, _string.Length - 2);
            return innerStr.Substring(0, 1);
        }
    }

    public DBusSignature DictEntryValue
    {
        get
        {
            if (IsDictEntry == false)
            {
                return "";
            }

            string innerStr = _string.Substring(1, _string.Length - 2);
            return innerStr.Substring(1);
        }
    }

    public bool IsContainer
    {
        get
        {
            if (IsStruct && IsDictEntry)
            {
                return true;
            }

            return false;
        }
    }

    public bool IsStruct
    {
        get { return _string.StartsWith("("); }
    }

    public bool IsDictEntry
    {
        get { return _string.StartsWith("{"); }
    }

    public bool IsArray
    {
        get { return _string.StartsWith("a"); }
    }

    public int Count => _signatures.Count;

    public override string ToString()
    {
        return _string;
    }

    public DBusSignature this[int index] => _signatures[index];

    public IEnumerator<DBusSignature> GetEnumerator()
    {
        foreach (string sig in _signatures)
        {
            yield return new DBusSignature(sig);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator DBusSignature(string str) => new DBusSignature(str);

    public static implicit operator string(DBusSignature signature) => signature._string;
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
            argsIter.AppendBasic(arg);
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
}
