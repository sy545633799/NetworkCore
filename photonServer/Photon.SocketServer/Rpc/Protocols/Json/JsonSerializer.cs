using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ExitGames.Logging;

namespace Photon.SocketServer.Rpc.Protocols.Json
{
    internal class JsonSerializer
    {
        // Fields
        private static readonly KeyValuePair<string, object> emptyKeyValue = new KeyValuePair<string, object>(null, null);
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        // Methods
        private static bool SerializeArray(IList list, TextWriter sw)
        {
            sw.Write('[');
            if (list.Count == 0)
            {
                sw.Write(']');
                return true;
            }
            SerializeValue(sw, list[0]);
            for (int i = 1; i < list.Count; i++)
            {
                sw.Write(',');
                if (!SerializeValue(sw, list[i]))
                {
                    return false;
                }
            }
            sw.Write(']');
            return true;
        }

        public static bool SerializeObject(IDictionary hashtable, TextWriter sw)
        {
            sw.Write('{');
            IDictionaryEnumerator enumerator = hashtable.GetEnumerator();
            if (enumerator.MoveNext())
            {
                SerializeString(enumerator.Key.ToString(), sw);
                sw.Write(':');
                if (!SerializeValue(sw, enumerator.Value))
                {
                    return false;
                }
            }
            while (enumerator.MoveNext())
            {
                sw.Write(",");
                SerializeString(enumerator.Key.ToString(), sw);
                sw.Write(":");
                if (!SerializeValue(sw, enumerator.Value))
                {
                    return false;
                }
            }
            sw.Write('}');
            return true;
        }

        public static bool SerializeObject(IDictionary<string, object> hashtable, TextWriter sw)
        {
            sw.Write('{');
            IEnumerator<KeyValuePair<string, object>> enumerator = hashtable.GetEnumerator();
            if (enumerator.MoveNext())
            {
                KeyValuePair<string, object> current = enumerator.Current;
                SerializeString(current.Key, sw);
                sw.Write(':');
                KeyValuePair<string, object> pair2 = enumerator.Current;
                if (!SerializeValue(sw, pair2.Value))
                {
                    return false;
                }
            }
            while (enumerator.MoveNext())
            {
                sw.Write(",");
                KeyValuePair<string, object> pair3 = enumerator.Current;
                SerializeString(pair3.Key, sw);
                sw.Write(":");
                KeyValuePair<string, object> pair4 = enumerator.Current;
                if (!SerializeValue(sw, pair4.Value))
                {
                    return false;
                }
            }
            sw.Write('}');
            return true;
        }

        private static void SerializeString(string s, TextWriter sw)
        {
            sw.Write("\"");
            foreach (char ch in s)
            {
                switch (ch)
                {
                    case '\b':
                        {
                            sw.Write(@"\b");
                            continue;
                        }
                    case '\t':
                        {
                            sw.Write(@"\t");
                            continue;
                        }
                    case '\n':
                        {
                            sw.Write(@"\n");
                            continue;
                        }
                    case '\f':
                        {
                            sw.Write(@"\f");
                            continue;
                        }
                    case '\r':
                        {
                            sw.Write(@"\r");
                            continue;
                        }
                    case '"':
                        {
                            sw.Write("\\\"");
                            continue;
                        }
                    case '\\':
                        {
                            sw.Write(@"\\");
                            continue;
                        }
                }
                if ((ch >= ' ') && (ch <= '\x007f'))
                {
                    sw.Write(ch);
                }
                else
                {
                    sw.Write(@"\u{0:X04}", (int)ch);
                }
            }
            sw.Write("\"");
        }

        internal static bool SerializeValue(TextWriter sw, object value)
        {
            if (value == null)
            {
                sw.Write("null");
                return true;
            }
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    sw.Write(!((bool)value) ? "false" : "true");
                    return true;

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    sw.Write(Convert.ToString(value, CultureInfo.InvariantCulture));
                    return true;

                case TypeCode.String:
                    SerializeString((string)value, sw);
                    return true;
            }
            IList list = value as IList;
            if (list != null)
            {
                return SerializeArray(list, sw);
            }
            IDictionary hashtable = value as IDictionary;
            if (hashtable != null)
            {
                return SerializeObject(hashtable, sw);
            }
            IDictionary<string, object> dictionary2 = value as IDictionary<string, object>;
            if (dictionary2 != null)
            {
                return SerializeObject(dictionary2, sw);
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Serialization of type {0} is not supported", new object[0]);
            }
            return false;
        }

        private static bool SkipWhiteSpace(string s, ref int index)
        {
            while (index < s.Length)
            {
                if (!char.IsWhiteSpace(s[index]))
                {
                    return false;
                }
                index++;
            }
            return true;
        }

        public static bool TryDeserialize(string s, out Dictionary<string, object> result, out int index)
        {
            index = 0;
            if (!TryFindNextToken(s, ref index, '{'))
            {
                result = null;
                return false;
            }
            result = new Dictionary<string, object>();
            while (index < s.Length)
            {
                KeyValuePair<string, object> pair;
                if (TryParseKeyValue(s, ref index, out pair))
                {
                    result.Add(pair.Key, pair.Value);
                    if (SkipWhiteSpace(s, ref index))
                    {
                        return false;
                    }
                    switch (s[index])
                    {
                        case ',':
                            {
                                index++;
                                continue;
                            }
                        case '}':
                            return true;
                    }
                }
                return false;
            }
            return false;
        }

        private static bool TryFindNextToken(string s, ref int index, char token)
        {
            if (SkipWhiteSpace(s, ref index))
            {
                return false;
            }
            if (s[index] != token)
            {
                return false;
            }
            index++;
            return true;
        }

        private static bool TryParseArray(string s, ref int index, out object value)
        {
            ArrayList list = new ArrayList();
            value = null;
            if (SkipWhiteSpace(s, ref index))
            {
                return false;
            }
            if (s[index] != ']')
            {
            Label_0088:
                if (index < s.Length)
                {
                    object obj2;
                    if (TryReadValue(s, ref index, out obj2))
                    {
                        list.Add(obj2);
                        if (SkipWhiteSpace(s, ref index))
                        {
                            return false;
                        }
                        switch (s[index])
                        {
                            case ',':
                                index++;
                                if (!SkipWhiteSpace(s, ref index))
                                {
                                    goto Label_0088;
                                }
                                return false;

                            case ']':
                                index++;
                                value = list.ToArray();
                                return true;
                        }
                    }
                    return false;
                }
                return false;
            }
            index++;
            value = new object[0];
            return true;
        }

        private static bool TryParseKeyValue(string s, ref int index, out KeyValuePair<string, object> keyValue)
        {
            string str;
            object obj2;
            if (!TryFindNextToken(s, ref index, '"'))
            {
                keyValue = emptyKeyValue;
                return false;
            }
            if (!TryParseString(s, ref index, out str))
            {
                keyValue = emptyKeyValue;
                return false;
            }
            if (!TryFindNextToken(s, ref index, ':'))
            {
                keyValue = emptyKeyValue;
                return false;
            }
            if (!TryReadValue(s, ref index, out obj2))
            {
                keyValue = emptyKeyValue;
                return false;
            }
            keyValue = new KeyValuePair<string, object>(str, obj2);
            return true;
        }

        private static bool TryParseNumber(string s, ref int index, out object value)
        {
            int num2;
            double num3;
            int startIndex = index;
            if (s[index] == '-')
            {
                index++;
            }
            while (index < s.Length)
            {
                if ((s[index] >= '0') && (s[index] <= '9'))
                {
                    index++;
                    continue;
                }
                switch (s[index])
                {
                    case '+':
                    case '-':
                    case '.':
                    case 'E':
                    case 'e':
                        break;

                    case ',':
                        goto Label_0077;

                    default:
                        goto Label_0077;
                }
                index++;
            }
        Label_0077:
            num2 = index - startIndex;
            if (double.TryParse(s.Substring(startIndex, num2), NumberStyles.Any, CultureInfo.InvariantCulture, out num3))
            {
                value = num3;
                return true;
            }
            value = (double)1.0 / (double)0.0;
            return false;
        }

        private static bool TryParseObject(string s, ref int index, out object value)
        {
            Hashtable hashtable = new Hashtable();
            value = hashtable;
            if (SkipWhiteSpace(s, ref index))
            {
                return false;
            }
            if (s[index] != '}')
            {
                KeyValuePair<string, object> pair;
            Label_0077:
                if (index >= s.Length)
                {
                    return true;
                }
                if (!TryParseKeyValue(s, ref index, out pair))
                {
                    return false;
                }
                hashtable.Add(pair.Key, pair.Value);
                if (!SkipWhiteSpace(s, ref index))
                {
                    switch (s[index])
                    {
                        case ',':
                            index++;
                            goto Label_0077;

                        case '}':
                            index++;
                            return true;
                    }
                    return false;
                }
                goto Label_0077;
            }
            index++;
            return true;
        }

        private static bool TryParseString(string s, ref int index, out string result)
        {
            StringBuilder builder = new StringBuilder();
            while (index < s.Length)
            {
                char ch = s[index];
                if (ch != '"')
                {
                    if (ch == '\\')
                    {
                        goto Label_0045;
                    }
                    builder.Append(s[index]);
                    goto Label_0171;
                }
                result = builder.ToString();
                index++;
                return true;
            Label_0045:
                if (index >= s.Length)
                {
                    result = null;
                    return false;
                }
                index++;
                switch (s[index])
                {
                    case '"':
                    case '/':
                    case '\\':
                        builder.Append(s[index]);
                        goto Label_0171;

                    case 'r':
                        builder.Append('\r');
                        goto Label_0171;

                    case 't':
                        builder.Append('\t');
                        goto Label_0171;

                    case 'u':
                        {
                            int num2;
                            index++;
                            int num = s.Length - index;
                            if ((num < 4) || !int.TryParse(s.Substring(index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num2))
                            {
                                break;
                            }
                            builder.Append(char.ConvertFromUtf32(num2));
                            index += 4;
                            continue;
                        }
                    case 'n':
                        builder.Append('\n');
                        goto Label_0171;

                    case 'b':
                        builder.Append('\b');
                        goto Label_0171;

                    case 'f':
                        builder.Append('\f');
                        goto Label_0171;

                    default:
                        builder.Append('\\');
                        builder.Append(s[index]);
                        goto Label_0171;
                }
                result = null;
                return false;
            Label_0171:
                index++;
            }
            result = null;
            return false;
        }

        private static bool TryReadValue(string s, ref int index, out object value)
        {
            if (SkipWhiteSpace(s, ref index))
            {
                value = null;
                return false;
            }
            switch (s[index])
            {
                case 'F':
                case 'f':
                    if (((s.Length - index) < 5) || (string.Compare("false", 0, s, index, 5, true) != 0))
                    {
                        break;
                    }
                    value = false;
                    index += 5;
                    return true;

                case 'N':
                case 'n':
                    if (((s.Length - index) >= 4) && (string.Compare("null", 0, s, index, 4, true) == 0))
                    {
                        value = null;
                        index += 4;
                        return true;
                    }
                    break;

                case 'T':
                case 't':
                    if (((s.Length - index) < 4) || (string.Compare("true", 0, s, index, 4, true) != 0))
                    {
                        break;
                    }
                    value = true;
                    index += 4;
                    return true;

                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return TryParseNumber(s, ref index, out value);

                case '"':
                    string str;
                    index++;
                    if (!TryParseString(s, ref index, out str))
                    {
                        break;
                    }
                    value = str;
                    return true;

                case '[':
                    index++;
                    return TryParseArray(s, ref index, out value);

                case '{':
                    index++;
                    return TryParseObject(s, ref index, out value);
            }
            value = null;
            return false;
        }
    }
}
