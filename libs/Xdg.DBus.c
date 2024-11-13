#include <dbus/dbus.h>

#ifndef _LANIAKEA_DBUS_C
#define _LANIAKEA_DBUS_C

dbus_bool_t la_dbus_message_append_int32_arg(DBusMessage *msg, int value)
{
    return dbus_message_append_args(msg, DBUS_TYPE_INT32, &value);
}

dbus_bool_t la_dbus_message_finish_arg(DBusMessage *msg)
{
    return dbus_message_append_args(msg, DBUS_TYPE_INVALID);
}

#endif /* _LANIAKEA_DBUS_C */
