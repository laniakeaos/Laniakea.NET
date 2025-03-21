using System.Text.RegularExpressions;
using System.Collections;

namespace Laniakea.Xdg.DBus;

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

    public static DBusSignature TypeToSignature(DBusType type)
    {
        return type switch
        {
            DBusType.Byte => "y",
            DBusType.Boolean => "b",
            DBusType.Int16 => "n",
            DBusType.UInt16 => "q",
            DBusType.Int32 => "i",
            DBusType.UInt32 => "u",
            DBusType.Int64 => "x",
            DBusType.UInt64 => "t",
            DBusType.Double => "d",
            DBusType.String => "s",
            DBusType.ObjectPath => "o",
            DBusType.Signature => "g",
            DBusType.UnixFD => "h",
            DBusType.Array => "a",
            DBusType.Variant => "v",
            DBusType.Struct => "r",
            DBusType.DictEntry => "e",
            _ => "",
        };
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
            if (IsStruct || IsDictEntry)
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

    public static DBusSignature operator +(DBusSignature left, DBusSignature right)
    {
        return new DBusSignature(left._string + right._string);
    }

    public static string operator +(string left, DBusSignature right)
    {
        return left + right._string;
    }

    public static string operator +(DBusSignature left, string right)
    {
        return left._string + right;
    }

    public static implicit operator DBusSignature(string str) => new DBusSignature(str);

    public static implicit operator string(DBusSignature signature) => signature._string;
}
