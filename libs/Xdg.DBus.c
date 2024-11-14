#include <stdint.h>
#include <stdlib.h>

#include <dbus/dbus.h>

#ifndef _LANIAKEA_DBUS_C
#define _LANIAKEA_DBUS_C

DBusMessageIter* la_dbus_message_iter_new(DBusMessage *msg)
{
    DBusMessageIter *args_iter = malloc(sizeof(DBusMessageIter));

    dbus_message_iter_init_append(msg, args_iter);

    return args_iter;
}

dbus_bool_t la_dbus_message_append_int32_arg(DBusMessage *msg,
                                             DBusMessageIter *iter,
                                             int value)
{
    return dbus_message_iter_append_basic(iter, DBUS_TYPE_INT32, &value);
}

dbus_bool_t la_dbus_message_append_uint32_arg(DBusMessage *msg,
                                              DBusMessageIter *iter,
                                              uint32_t value)
{
    return dbus_message_iter_append_basic(iter, DBUS_TYPE_UINT32, &value);
}

dbus_bool_t la_dbus_message_finish_arg(DBusMessage *msg)
{
    return dbus_message_append_args(msg, DBUS_TYPE_INVALID);
}

#endif /* _LANIAKEA_DBUS_C */
