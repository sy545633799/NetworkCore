using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Wodsoft.Net.Communication;

namespace Wodsoft.Net.Service
{
    public class DataFormatter
    {
        public BinaryFormatter Formatter;

        public DataFormatter()
        {
            Formatter = new BinaryFormatter();
        }

        public virtual object Deserialize(Type objType, byte[] data)
        {
            Type type = objType;

            //byte[]类型直接返回
            if (type == typeof(byte[]))
                return data;

            //值类型反序列化
            if (type.IsValueType || type == typeof(string))
            {
                MemoryStream ms = new MemoryStream(data);
                return Formatter.Deserialize(ms);
            }

            BinaryDataReader reader = new BinaryDataReader(data);

            //数组类型反序列化
            if (type.IsArray)
            {                
                Type element = type.GetElementType();
                Array array = Array.CreateInstance(type, reader.ReadByte());
                for (int i = 0; i < array.Length; i++)
                    array.SetValue(Deserialize(element, reader.ReadBytes()), i);
                return array;
            }

            //泛型反序列化
            if (type.IsGenericType)
            {

            }

            //复杂类序列化
            object obj = Activator.CreateInstance(type);
            var properties = type.GetProperties();
            while (!reader.IsEndOfData)
            {
                var property = properties[reader.ReadByte()];
                property.SetValue(obj, Deserialize(property.PropertyType, reader.ReadBytes()), null);
            }

            return obj;
        }

        public T Deserialize<T>(byte[] data) where T : new()
        {
            return (T)Deserialize(typeof(T), data);
        }

        public virtual byte[] Serialize(object obj)
        {
            //null值
            if (obj == null)
                return new byte[0];

            Type type = obj.GetType();

            //byte[]直接返回
            if (type == typeof(byte[]))
                return (byte[])obj;
                        
            BinaryDataWriter writer = new BinaryDataWriter();
        
            //值类型序列化
            if (type.IsValueType || type == typeof(string))
            {
                MemoryStream stream = new MemoryStream();
                Formatter.Serialize(stream, obj);
                return stream.ToArray();
            }

            //数组序列化
            if (type.IsArray)
            {
                Array array = (Array)obj;
                writer.WriteInt32(array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    var data = Serialize(array.GetValue(i));
                    writer.WriteBytes(data);
                }
                return writer.ToArray();
            }

            //泛型序列化
            if (type.IsGenericType)
            {
                
            }

            //复杂类序列化
            var properties = type.GetProperties();
            for (byte i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                #region 判断是否有效数据

                if (!property.CanRead || !property.CanWrite)
                    continue;

                #endregion

                writer.WriteByte(i);

                var data = Serialize(property.GetValue(obj, null));
                writer.WriteBytes(data);
            }

            return writer.ToArray();
        }
    }
}
