using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ExitGames.IO
{
    /// <summary>
    /// This class converts binary data to streams and vice versa and de-/serialzes objects with the <see cref="T:System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static class BinaryConverter
    {
        /// <summary>
        ///  The thread static formatter.
        /// </summary>
        [ThreadStatic]
        private static IFormatter formatter;

        /// <summary>
        ///    Converts a stream to a byte array.
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="length">  The length.</param>
        /// <returns>A byte array.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="data"/> is null.
        ///  </exception>
        public static byte[] ConvertStreamToByteArray(Stream data, int length)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (data.CanSeek)
            {
                data.Position = 0;
            }
            return ReadBytesFromStream(data, length);
        }

        /// <summary>
        ///  Converts a byte array to an object of type T with the <see cref="T:System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>.
        /// </summary>
        /// <typeparam name="T">The bytes.</typeparam>
        /// <param name="bytes">The object type.</param>
        /// <returns> An object of type T.</returns>
        public static T Deserialize<T>(byte[] bytes)
        {
            T local;
            if (bytes == null)
            {
                return default(T);
            }
            MemoryStream stream = new MemoryStream(bytes);
            try
            {
                local = (T)GetBinaryFormatterThreadStatic().Deserialize(stream);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return local;
        }

        /// <summary>
        /// Converts a byte array to an object of type T with the <see cref="T:System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="bytes"> A serialized object of Type  T.</param>
        /// <param name="index">     The start index.</param>
        /// <param name="count"> The length to the serialized object.</param>
        /// <returns> An object of type T.</returns>
        public static T Deserialize<T>(byte[] bytes, int index, int count)
        {
            T local;
            if (bytes == null)
            {
                return default(T);
            }
            MemoryStream stream = new MemoryStream(bytes, index, count);
            try
            {
                local = (T)GetBinaryFormatterThreadStatic().Deserialize(stream);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return local;
        }

        /// <summary>
        ///  Gets a thread static <see cref="T:System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>.
        /// </summary>
        /// <returns>A thread static <see cref="T:System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>.</returns>
        public static IFormatter GetBinaryFormatterThreadStatic()
        {
            return formatter ?? (formatter = new BinaryFormatter());
        }

        /// <summary>
        ///  Reads the given amount of bytes from a stream.
        /// </summary>
        /// <param name="data"> The stream.</param>
        /// <param name="length">The length to read.</param>
        /// <returns>A byte array.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///  <paramref name="data"/> is null.
        ///  </exception>
        public static byte[] ReadBytesFromStream(Stream data, int length)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            byte[] buffer = new byte[length];
            int num = length;
            int num2 = 0;
            int num3 = 1;
            while ((num > 0) && (num3 > 0))
            {
                num3 = data.Read(buffer, num2, num);
                num2 += num3;
                num -= num3;
            }
            return buffer;
        }

        /// <summary>
        /// Serializes an object of type T.
        /// </summary>
        /// <typeparam name="T"> The object type.</typeparam>
        /// <param name="data"> The object to serialize.</param>
        /// <returns> A byte array.</returns>
        public static byte[] Serialize<T>(ref T data)
        {
            byte[] buffer;
            MemoryStream stream = new MemoryStream();
            try
            {
                GetBinaryFormatterThreadStatic().Serialize(stream, (T)data);
                buffer = stream.ToArray();
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return buffer;
        }
    }
}
