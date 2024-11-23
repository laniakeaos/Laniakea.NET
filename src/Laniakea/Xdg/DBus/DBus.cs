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

internal static class DBusMessageIterProcessor
{
    internal static List<DBusArgument> ProcessIterToList(IntPtr iter)
    {
        List<DBusArgument> arguments = new List<DBusArgument>();

        do
        {
            int argType = CDBus.dbus_message_iter_get_arg_type(iter);
            switch (argType)
            {
                case CDBus.DBUS_TYPE_STRING:
                    IntPtr val = IntPtr.Zero;
                    CDBus.dbus_message_iter_get_basic(iter, ref val);
                    string str = Marshal.PtrToStringAnsi(val)!;
                    arguments.Add(new DBusArgument(str));
                    break;
                default:
                    break;
            }
            CDBus.dbus_message_iter_next(iter);
        } while (CDBus.dbus_message_iter_has_next(iter) == 1);

        return arguments;
    }

    internal static void ProcessIterFromList(List<DBusArgument> arguments, DBusSignature signature, IntPtr iter)
    {
        if (arguments.Count != signature.Count)
        {
            throw new DBusException("Argument count mismatch.");
        }

        void CheckArgumentSignature(DBusType argType, DBusSignature argSig)
        {
            if (argType == DBusType.String && argSig == "s")
            {
                return;
            } else if (argType == DBusType.Int32 && argSig == "i")
            {
                return;
            }

            throw new DBusException("Invalid argument signature.");
        }

        for (int i = 0; i < arguments.Count; ++i)
        {
            DBusArgument argument = arguments[i];
            DBusSignature argSignature = signature[i];
            int sig = 0;
            if (argSignature.ToString().Length == 1)
            {
                sig = argSignature.ToString()[0];
            }
            CheckArgumentSignature(argument.Type, argSignature);

            if (sig == CDBus.DBUS_TYPE_BYTE)
            {
                byte b = (byte)argument.Value;
                IntPtr bPtr = Marshal.AllocHGlobal(sizeof(byte));
                Marshal.WriteByte(bPtr, b);
                CDBus.dbus_message_iter_append_basic(iter, CDBus.DBUS_TYPE_BYTE, bPtr);
                Marshal.FreeHGlobal(bPtr);
            } else if (sig == CDBus.DBUS_TYPE_BOOLEAN)
            {
                bool b = (bool)argument.Value;
                IntPtr bPtr = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(bPtr, b == true ? 1 : 0);
                CDBus.dbus_message_iter_append_basic(iter, CDBus.DBUS_TYPE_BOOLEAN, bPtr);
                Marshal.FreeHGlobal(bPtr);
            } else if (sig == CDBus.DBUS_TYPE_INT16)
            {
                Int16 i16 = (Int16)argument.Value;
                IntPtr iPtr = Marshal.AllocHGlobal(sizeof(Int16));
                Marshal.WriteInt16(iPtr, i16);
                CDBus.dbus_message_iter_append_basic(iter, CDBus.DBUS_TYPE_INT16, iPtr);
                Marshal.FreeHGlobal(iPtr);
            } else if (sig == CDBus.DBUS_TYPE_UINT16)
            {
                UInt16 u16 = (UInt16)argument.Value;
                IntPtr uPtr = Marshal.AllocHGlobal(sizeof(UInt16));
                Marshal.WriteInt32(uPtr, u16);
                CDBus.dbus_message_iter_append_basic(iter, CDBus.DBUS_TYPE_UINT16, uPtr);
                Marshal.FreeHGlobal(uPtr);
            } else if (sig == CDBus.DBUS_TYPE_STRING)
            {
                string s = (string)argument.Value;
                IntPtr sPtr = Marshal.StringToHGlobalAnsi(s);
                CDBus.dbus_message_iter_append_basic(iter, CDBus.DBUS_TYPE_STRING, sPtr);
                Marshal.FreeHGlobal(sPtr);
            } else if (sig == CDBus.DBUS_TYPE_INT32)
            {
                int i32 = (int)argument.Value;
                IntPtr iPtr = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(iPtr, i32);
                CDBus.dbus_message_iter_append_basic(iter, CDBus.DBUS_TYPE_INT32, iPtr);
                Marshal.FreeHGlobal(iPtr);
            } else if (sig == CDBus.DBUS_TYPE_UINT32)
            {
                uint u32 = (uint)argument.Value;
                IntPtr uPtr = Marshal.AllocHGlobal(sizeof(uint));
                Marshal.WriteInt64(uPtr, u32);
                CDBus.dbus_message_iter_append_basic(iter, CDBus.DBUS_TYPE_UINT32, uPtr);
                Marshal.FreeHGlobal(uPtr);
            } else if (sig == CDBus.DBUS_TYPE_INT64)
            {
                Int64 i64 = (Int64)argument.Value;
                IntPtr iPtr = Marshal.AllocHGlobal(sizeof(Int64));
                Marshal.WriteInt64(iPtr, i64);
                CDBus.dbus_message_iter_append_basic(iter, CDBus.DBUS_TYPE_INT64, iPtr);
                Marshal.FreeHGlobal(iPtr);
            } else if (sig == CDBus.DBUS_TYPE_UINT64)
            {
                UInt64 u64 = (UInt64)argument.Value;
                IntPtr uPtr = Marshal.AllocHGlobal(sizeof(UInt64));
                Marshal.WriteInt64(uPtr, unchecked((Int64)u64));
                CDBus.dbus_message_iter_append_basic(iter, CDBus.DBUS_TYPE_UINT64, uPtr);
                Marshal.FreeHGlobal(uPtr);
            }
        }
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
            if (IsArray() == false)
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
            if (IsStruct() == false)
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
            if (IsDictEntry() == false)
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
            if (IsDictEntry() == false)
            {
                return "";
            }
            
            string innerStr = _string.Substring(1, _string.Length - 2);
            return innerStr.Substring(1);
        }
    }

    public bool IsContainer()
    {
        if (IsStruct() && IsDictEntry())
        {
            return true;
        }

        return false;
    }

    public bool IsStruct()
    {
        return _string.StartsWith("(");
    }

    public bool IsDictEntry()
    {
        return _string.StartsWith("{");
    }

    public bool IsArray()
    {
        return _string.StartsWith("a");
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
        IntPtr argsIter = CDBus.la_dbus_message_iter_new();
        CDBus.dbus_message_iter_init_append(message._cPtr, argsIter);
        foreach (var arg in message.Arguments)
        {
            switch (arg.Type)
            {
                case DBusType.Int32:
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
        retMsg.Type = DBusMessageType.MethodReturn;

        // Get and set the signature.
        IntPtr sig = CDBus.dbus_message_get_signature(cMsg);
        string? signature = Marshal.PtrToStringAnsi(sig);
        if (signature != null)
        {
            retMsg.Signature = signature;
            IntPtr iter = CDBus.la_dbus_message_iter_new();
            CDBus.dbus_message_iter_init(cMsg, iter);
            var args = DBusMessageIterProcessor.ProcessIterToList(iter);
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