using System;
using System.Xml;

namespace Laniakea.Xdg.DBus;

public enum DBusArgumentDirection
{
    In,
    Out,
}

public class DBusInOutArgument
{
    public string Name { get; init; }
    public DBusArgumentDirection Direction { get; init; }
    public DBusType Type { get; init; }
    public string Signature { get; set; } = string.Empty;

    public DBusInOutArgument(string name, DBusArgumentDirection direction, DBusType type)
    {
        Name = name;
        Direction = direction;
        Type = type;
    }

    public DBusInOutArgument(string name, DBusArgumentDirection direction, string signature)
    {
        Name = name;
        Direction = direction;
        Signature = signature;
    }
}

public enum DBusObjectType
{
    Node,
    Interface,
}

public enum DBusMemberType
{
    Method,
    Property,
    Signal,
}

public enum DBusPropertyAccess
{
    Read,
    ReadWrite,
}

public abstract class DBusObject
{
    public string Name { get; init; } = string.Empty;
    public DBusObjectType ObjectType { get; init; }

    public DBusObject(string name)
    {
        Name = name;
        ObjectType = DBusObjectType.Node;
    }
}

public abstract class DBusMember
{
    public string Name { get; init; } = string.Empty;
    public DBusMemberType MemberType { get; init; }

    public DBusMember(string name, DBusMemberType type)
    {
        Name = name;
        MemberType = type;
    }
}

public class DBusInterface : DBusObject
{
    public List<DBusMember> Members { get; init; } = new List<DBusMember>();

    public List<DBusMethod> Methods
    {
        get
        {
            var ret = new List<DBusMethod>();
            var methods = Members.Where(m => m.MemberType == DBusMemberType.Method).ToList();
            foreach (var method in methods)
            {
                ret.Add((DBusMethod)method);
            }

            return ret;
        }
    }

    public List<DBusProperty> Properties
    {
        get
        {
            var ret = new List<DBusProperty>();
            var properties = Members.Where(m => m.MemberType == DBusMemberType.Property).ToList();
            foreach (var property in properties)
            {
                ret.Add((DBusProperty)property);
            }

            return ret;
        }
    }

    public List<DBusSignal> Signals
    {
        get
        {
            var ret = new List<DBusSignal>();
            var signals = Members.Where(m => m.MemberType == DBusMemberType.Signal).ToList();
            foreach (var signal in signals)
            {
                ret.Add((DBusSignal)signal);
            }

            return ret;
        }
    }

    public DBusInterface(string name) : base(name)
    {
        base.ObjectType = DBusObjectType.Interface;
    }

    public override string ToString()
    {
        var properties = Properties.Count;
        var signals = Signals.Count;
        var methods = Methods.Count;
        return "<Interface Name=" + Name + ", Properties=" + properties + ", Signals=" + signals + ", Methods=" +
               methods + ", Total=" + Members.Count + ">";
    }
}

public class DBusNode : DBusObject
{
    public DBusNode(string name) : base(name)
    {
        base.ObjectType = DBusObjectType.Node;
    }

    public override string ToString()
    {
        return "<Node Name=" + Name + ">";
    }
}

public class DBusMethod : DBusMember
{
    public List<DBusInOutArgument> InArguments { get; set; } = new List<DBusInOutArgument>();
    public List<DBusInOutArgument> OutArguments { get; set; } = new List<DBusInOutArgument>();

    public DBusMethod(string name) : base(name, DBusMemberType.Method)
    {
    }
}

public class DBusSignal : DBusMember
{
    public List<DBusInOutArgument> Arguments { get; init; } = new List<DBusInOutArgument>();

    public DBusSignal(string name) : base(name, DBusMemberType.Signal)
    {
    }
}

public class DBusProperty : DBusMember
{
    public DBusPropertyAccess Access { get; set; }
    public string TypeSignature { get; set; } = string.Empty;

    public DBusProperty(string name) : base(name, DBusMemberType.Property)
    {
        Access = DBusPropertyAccess.Read;
    }

    public DBusProperty(string name, DBusPropertyAccess access) : base(name, DBusMemberType.Property)
    {
        Access = access;
    }
}

public class DBusIntrospection
{
    public string XmlString { get; set; } = string.Empty;
    public List<DBusObject> Objects { get; } = new List<DBusObject>();

    public List<DBusInterface> Interfaces
    {
        get
        {
            var ifaces = Objects.FindAll(n => n.ObjectType == DBusObjectType.Interface);
            List<DBusInterface> ret = new List<DBusInterface>();
            foreach (DBusObject iface in ifaces)
            {
                ret.Add((DBusInterface)iface);
            }

            return ret;
        }
    }

    public List<DBusNode> Nodes
    {
        get
        {
            var nodes = Objects.FindAll(n => n.ObjectType == DBusObjectType.Node);
            List<DBusNode> ret = new List<DBusNode>();
            foreach (DBusObject node in nodes)
            {
                ret.Add((DBusNode)node);
            }

            return ret;
        }
    }

    public DBusIntrospection(string xmlString)
    {
        XmlString = xmlString;

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlString);

        XmlElement? root = doc.DocumentElement;
        if (root == null)
        {
            return;
        }

        foreach (XmlElement node in root.ChildNodes)
        {
            if (node.Name == "interface")
            {
                var iface = new DBusInterface(node.GetAttribute("name"));
                Objects.Add(iface);
                foreach (XmlElement member in node.ChildNodes)
                {
                    switch (member.Name)
                    {
                        case "method":
                        {
                            var method = new DBusMethod(member.GetAttribute("name"));
                            // Add in/out arguments.
                            foreach (XmlElement arg in member.ChildNodes)
                            {
                                if (arg.Name != "arg")
                                {
                                    continue;
                                }
                                string directionStr = arg.GetAttribute("direction");
                                string typeStr = arg.GetAttribute("type");
                                string name = arg.GetAttribute("name");
                                DBusArgumentDirection direction = (directionStr == "in") ? DBusArgumentDirection.In : DBusArgumentDirection.Out;
                                var argument = new DBusInOutArgument(name, direction, typeStr);
                                if (argument.Direction == DBusArgumentDirection.In)
                                {
                                    method.InArguments.Add(argument);
                                }
                                else
                                {
                                    method.OutArguments.Add(argument);
                                }
                            }
                            iface.Members.Add(method);
                            break;
                        }
                        case "property":
                        {
                            var property = new DBusProperty(member.GetAttribute("name"));
                            // Property access.
                            {
                                var access = member.GetAttribute("access");
                                if (access == "read")
                                {
                                    property.Access = DBusPropertyAccess.Read;
                                }
                                else if (access == "readwrite")
                                {
                                    property.Access = DBusPropertyAccess.ReadWrite;
                                }
                            }
                            // Property type (signature).
                            {
                                var sig = member.GetAttribute("type");
                                property.TypeSignature = sig;
                            }
                            iface.Members.Add(property);
                            break;
                        }
                        case "signal":
                        {
                            var signal = new DBusSignal(member.GetAttribute("name"));
                            foreach (XmlElement arg in member.ChildNodes)
                            {
                                if (arg.Name != "arg")
                                {
                                    // Skip annotation.
                                    continue;
                                }
                                string directionStr = arg.GetAttribute("direction");
                                DBusArgumentDirection direction = directionStr == "out" ? DBusArgumentDirection.Out : DBusArgumentDirection.In;
                                string typeStr = arg.GetAttribute("type");
                                string name = arg.GetAttribute("name");
                                var argument = new DBusInOutArgument(name, direction, typeStr);
                                signal.Arguments.Add(argument);
                            }
                            iface.Members.Add(signal);
                            break;
                        }
                    }
                }
            } else if (node.Name == "node")
            {
                var n = new DBusNode(node.GetAttribute("name"));
                Objects.Add(n);
            }
        }
    }

    public override string ToString()
    {
        return "<DBusIntrospection, " + Objects.Count + " Objects>";
    }
}
