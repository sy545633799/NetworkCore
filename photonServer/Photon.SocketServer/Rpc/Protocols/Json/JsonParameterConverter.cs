using System;
using System.Collections;
using System.Collections.Generic;
using Photon.SocketServer.Rpc.Reflection;

namespace Photon.SocketServer.Rpc.Protocols.Json
{
    internal class JsonParameterConverter
    {
        // Methods
        public static bool TryConvertOperationParameter(ObjectMemberInfo<DataMemberAttribute> paramterInfo, ref object value)
        {
            if (paramterInfo.TypeCode != TypeCode.Object)
            {
                return TryConvertParamter(paramterInfo.TypeCode, ref value);
            }
            if (paramterInfo.ValueType == typeof(Hashtable))
            {
                return (value is Hashtable);
            }
            if (paramterInfo.ValueType == typeof(Guid))
            {
                return TryConvertToGuid(ref value);
            }
            if (paramterInfo.ValueType.IsArray)
            {
                IList array = value as IList;
                if (array == null)
                {
                    return false;
                }
                switch (Type.GetTypeCode(paramterInfo.ValueType.GetElementType()))
                {
                    case TypeCode.Byte:
                        return TryConvertToByteArray(ref value, array);

                    case TypeCode.Int16:
                        return TryConvertToShortArray(ref value, array);

                    case TypeCode.Int32:
                        return TryConvertToIntArray(ref value, array);

                    case TypeCode.Int64:
                        return TryConvertToLongArray(ref value, array);

                    case TypeCode.Single:
                        return TryConvertToSingleArray(ref value, array);

                    case TypeCode.Double:
                        return TryConvertToDoubleArray(ref value, array);

                    case TypeCode.String:
                        return TryConvertToStringArray(ref value, array);
                }
                if (paramterInfo.ValueType == typeof(Hashtable[]))
                {
                    return TryConvertToHashtableArray(ref value, array);
                }
            }
            if (paramterInfo.ValueType == typeof(List<int>))
            {
                return TryConvertToIntList(ref value);
            }
            return true;
        }

        private static bool TryConvertParamter(TypeCode code, ref object value)
        {
            switch (code)
            {
                case TypeCode.Boolean:
                    return (value is bool);

                case TypeCode.Byte:
                    if (!(value is double))
                    {
                        return false;
                    }
                    value = (byte)((double)value);
                    return true;

                case TypeCode.Int16:
                    if (!(value is double))
                    {
                        return false;
                    }
                    value = (short)((double)value);
                    return true;

                case TypeCode.Int32:
                    if (!(value is double))
                    {
                        return false;
                    }
                    value = (int)((double)value);
                    return true;

                case TypeCode.Int64:
                    if (!(value is double))
                    {
                        return false;
                    }
                    value = (long)((double)value);
                    return true;

                case TypeCode.Single:
                    if (!(value is double))
                    {
                        return false;
                    }
                    value = (float)((double)value);
                    return true;

                case TypeCode.Double:
                    if (!(value is double))
                    {
                        return false;
                    }
                    value = (double)value;
                    return true;

                case TypeCode.String:
                    return (value is string);
            }
            return false;
        }

        private static bool TryConvertToByteArray(ref object value, IList array)
        {
            byte[] buffer = new byte[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                object obj2 = array[i];
                if (obj2 is double)
                {
                    buffer[i] = (byte)((double)obj2);
                }
                else
                {
                    return false;
                }
            }
            value = buffer;
            return true;
        }

        private static bool TryConvertToDoubleArray(ref object value, IList array)
        {
            double[] numArray = new double[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                object obj2 = array[i];
                if (obj2 is double)
                {
                    numArray[i] = (double)obj2;
                }
                else
                {
                    return false;
                }
            }
            value = numArray;
            return true;
        }

        private static bool TryConvertToGuid(ref object value)
        {
            string g = value as string;
            if (g == null)
            {
                return false;
            }
            try
            {
                value = new Guid(g);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static bool TryConvertToHashtableArray(ref object value, IList array)
        {
            Hashtable[] hashtableArray = new Hashtable[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                Hashtable hashtable = array[i] as Hashtable;
                if (hashtable != null)
                {
                    hashtableArray[i] = hashtable;
                }
                else
                {
                    return false;
                }
            }
            value = hashtableArray;
            return true;
        }

        private static bool TryConvertToIntArray(ref object value, IList array)
        {
            int[] numArray = new int[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                object obj2 = array[i];
                if (obj2 is double)
                {
                    numArray[i] = (int)((double)obj2);
                }
                else
                {
                    return false;
                }
            }
            value = numArray;
            return true;
        }

        private static bool TryConvertToIntList(ref object value)
        {
            object[] objArray = value as object[];
            if (objArray == null)
            {
                return false;
            }
            List<int> list = new List<int>(objArray.Length);
            for (int i = 0; i < objArray.Length; i++)
            {
                object obj2 = objArray[i];
                if (obj2 is double)
                {
                    list.Add((int)((double)obj2));
                }
                else
                {
                    return false;
                }
            }
            value = list;
            return true;
        }

        private static bool TryConvertToLongArray(ref object value, IList array)
        {
            long[] numArray = new long[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                object obj2 = array[i];
                if (obj2 is double)
                {
                    numArray[i] = (long)((double)obj2);
                }
                else
                {
                    return false;
                }
            }
            value = numArray;
            return true;
        }

        private static bool TryConvertToShortArray(ref object value, IList array)
        {
            short[] numArray = new short[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                object obj2 = array[i];
                if (obj2 is double)
                {
                    numArray[i] = (short)((double)obj2);
                }
                else
                {
                    return false;
                }
            }
            value = numArray;
            return true;
        }

        private static bool TryConvertToSingleArray(ref object value, IList array)
        {
            float[] numArray = new float[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                object obj2 = array[i];
                if (obj2 is double)
                {
                    numArray[i] = (float)((double)obj2);
                }
                else
                {
                    return false;
                }
            }
            value = numArray;
            return true;
        }

        private static bool TryConvertToStringArray(ref object value, IList array)
        {
            string[] strArray = new string[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                string str = array[i] as string;
                if (str != null)
                {
                    strArray[i] = str;
                }
                else
                {
                    return false;
                }
            }
            value = strArray;
            return true;
        }
    }
}
