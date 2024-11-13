PKG_CONFIG=`pkg-config --cflags --libs dbus-1`

default:
	$(CC) -shared -fPIC $(PKG_CONFIG) Xdg.DBus.c -o libXdg.DBus.so

clean:
	rm *.so
