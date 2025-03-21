using System.Runtime.InteropServices;

using Laniakea.Xdg.DBus.CApi;

namespace Laniakea.Xdg.DBus;

internal class DBusMessageIter
{
    private CDBus.DBusMessageIter _cIter;
    private DBusMessageIter? _subIter = null;
    private IntPtr _cMessage = IntPtr.Zero;

    public DBusMessageIter()
    {
        _cIter = new CDBus.DBusMessageIter();
    }

    ~DBusMessageIter()
    {
    }

    public static DBusMessageIter InitAppend(IntPtr message)
    {
        DBusMessageIter iter = new DBusMessageIter();
        iter._cMessage = message;
        CDBus.dbus_message_iter_init_append(message, ref iter._cIter);

        return iter;
    }

    public static DBusMessageIter Init(IntPtr message)
    {
        DBusMessageIter iter = new DBusMessageIter();
        iter._cMessage = message;
        CDBus.dbus_message_iter_init(message, ref iter._cIter);

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
                result = CDBus.dbus_message_iter_append_basic(ref _cIter, CDBus.DBUS_TYPE_BYTE, cPtr);
                break;
            case DBusType.Boolean:
                cPtr = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(cPtr, (bool)arg.Value == true ? 1 : 0);
                result = CDBus.dbus_message_iter_append_basic(ref _cIter, CDBus.DBUS_TYPE_BOOLEAN, cPtr);
                break;
            case DBusType.Int16:
                cPtr = Marshal.AllocHGlobal(sizeof(Int16));
                Marshal.WriteInt16(cPtr, (Int16)arg.Value);
                result = CDBus.dbus_message_iter_append_basic(ref _cIter, CDBus.DBUS_TYPE_INT16, cPtr);
                break;
            case DBusType.UInt16:
                cPtr = Marshal.AllocHGlobal(sizeof(UInt16));
                Marshal.WriteInt16(cPtr, unchecked((Int16)((UInt16)arg.Value)));
                result = CDBus.dbus_message_iter_append_basic(ref _cIter, CDBus.DBUS_TYPE_UINT16, cPtr);
                break;
            case DBusType.Int32:
                cPtr = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(cPtr, (int)arg.Value);
                result = CDBus.dbus_message_iter_append_basic(ref _cIter, CDBus.DBUS_TYPE_INT32, cPtr);
                break;
            case DBusType.UInt32:
                cPtr = Marshal.AllocHGlobal(sizeof(uint));
                Marshal.WriteInt32(cPtr, unchecked((int)((uint)arg.Value)));
                result = CDBus.dbus_message_iter_append_basic(ref _cIter, CDBus.DBUS_TYPE_UINT32, cPtr);
                break;
            case DBusType.Int64:
                cPtr = Marshal.AllocHGlobal(sizeof(Int64));
                Marshal.WriteInt64(cPtr, (Int64)arg.Value);
                result = CDBus.dbus_message_iter_append_basic(ref _cIter, CDBus.DBUS_TYPE_INT64, cPtr);
                break;
            case DBusType.UInt64:
                cPtr = Marshal.AllocHGlobal(sizeof(UInt64));
                Marshal.WriteInt64(cPtr, unchecked((Int64)((UInt64)arg.Value)));
                result = CDBus.dbus_message_iter_append_basic(ref _cIter, CDBus.DBUS_TYPE_UINT64, cPtr);
                break;
            case DBusType.Double:
                cPtr = Marshal.AllocHGlobal(sizeof(double));
                Marshal.StructureToPtr<double>((double)arg.Value, cPtr, false);
                result = CDBus.dbus_message_iter_append_basic(ref _cIter, CDBus.DBUS_TYPE_DOUBLE, cPtr);
                break;
            case DBusType.String:
                cPtr = Marshal.StringToHGlobalAnsi((string)arg.Value);
                result = CDBus.dbus_message_iter_append_basic(ref _cIter, CDBus.DBUS_TYPE_STRING, cPtr);
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

        int result = CDBus.dbus_message_iter_open_container(ref _cIter, cType, signature?.ToString(), ref _subIter._cIter);

        return result == 1 ? true : false;
    }

    public bool CloseContainer()
    {
        if (_subIter == null)
        {
            throw new DBusInternalException("Can't close container because there is no sub-iter.");
        }
        int result = CDBus.dbus_message_iter_close_container(ref _cIter, ref _subIter._cIter);

        return result == 1 ? true : false;
    }

    public List<DBusArgument> ToArgumentList()
    {
        List<DBusArgument> list = [];

        do
        {
            IntPtr cPtr = IntPtr.Zero;

            int argType = CDBus.dbus_message_iter_get_arg_type(ref _cIter);
            switch (argType)
            {
                case CDBus.DBUS_TYPE_BYTE:
                    CDBus.dbus_message_iter_get_basic(ref _cIter, ref cPtr);
                    byte val = Marshal.PtrToStructure<byte>(cPtr);
                    list.Add(new DBusArgument(val));
                    break;
                case CDBus.DBUS_TYPE_BOOLEAN:
                    CDBus.dbus_message_iter_get_basic(ref _cIter, ref cPtr);
                    int boolVal = Marshal.ReadInt32(cPtr);
                    list.Add(new DBusArgument(boolVal == 1 ? true : false));
                    break;
                case CDBus.DBUS_TYPE_INT16:
                    CDBus.dbus_message_iter_get_basic(ref _cIter, ref cPtr);
                    Int16 i16Val = Marshal.ReadInt16(cPtr);
                    list.Add(new DBusArgument(i16Val));
                    break;
                case CDBus.DBUS_TYPE_UINT16:
                    CDBus.dbus_message_iter_get_basic(ref _cIter, ref cPtr);
                    UInt16 u16Val = unchecked((UInt16)Marshal.ReadInt16(cPtr));
                    list.Add(new DBusArgument(u16Val));
                    break;
                case CDBus.DBUS_TYPE_INT32:
                    CDBus.dbus_message_iter_get_basic(ref _cIter, ref cPtr);
                    int i32Val = Marshal.ReadInt32(cPtr);
                    list.Add(new DBusArgument(i32Val));
                    break;
                case CDBus.DBUS_TYPE_UINT32:
                    CDBus.dbus_message_iter_get_basic(ref _cIter, ref cPtr);
                    uint u32Val = unchecked((uint)Marshal.ReadInt32(cPtr));
                    list.Add(new DBusArgument(u32Val));
                    break;
                case CDBus.DBUS_TYPE_INT64:
                    CDBus.dbus_message_iter_get_basic(ref _cIter, ref cPtr);
                    Int64 i64Val = Marshal.ReadInt64(cPtr);
                    list.Add(new DBusArgument(i64Val));
                    break;
                case CDBus.DBUS_TYPE_UINT64:
                    CDBus.dbus_message_iter_get_basic(ref _cIter, ref cPtr);
                    UInt64 u64Val = unchecked((UInt64)Marshal.ReadInt64(cPtr));
                    list.Add(new DBusArgument(u64Val));
                    break;
                case CDBus.DBUS_TYPE_DOUBLE:
                    CDBus.dbus_message_iter_get_basic(ref _cIter, ref cPtr);
                    double dVal = Marshal.PtrToStructure<double>(cPtr);
                    list.Add(new DBusArgument(dVal));
                    break;
                case CDBus.DBUS_TYPE_STRING:
                    CDBus.dbus_message_iter_get_basic(ref _cIter, ref cPtr);
                    string str = Marshal.PtrToStringAnsi(cPtr)!;
                    list.Add(new DBusArgument(str));
                    break;
                default:
                    break;
            }
            CDBus.dbus_message_iter_next(ref _cIter);
        } while (CDBus.dbus_message_iter_has_next(ref _cIter) == 1);

        return list;
    }
}
