namespace ExitGames.Client.Photon
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Contains several (more or less) useful static methods, mostly used for debugging.
    /// </summary>
    public class SupportClass
    {
        protected internal static IntegerMillisecondsDelegate IntegerMilliseconds = delegate
        {
            return Environment.TickCount;
        };

        ///<summary>
        /// Gets the local machine's "milliseconds since start" value (precision is described in remarks).
        ///</summary>
        ///<remarks>
        /// This method uses Environment.TickCount (cheap but with only 16ms precision).
        /// PhotonPeer.LocalMsTimestampDelegate is available to set the delegate (unless already connected).
        ///</remarks>
        ///<returns>Fraction of the current time in Milliseconds (this is not a proper datetime timestamp).</returns>
        public static int GetTickCount()
        {
            return IntegerMilliseconds();
        }

        ///<summary>
        /// Writes the exception's stack trace to the received stream.
        ///</summary>
        ///<param name="throwable">Exception to obtain information from.</param>
        ///<param name="stream">Output sream used to write to.</param>
        public static void WriteStackTrace(Exception throwable, [Optional, DefaultParameterValue(null)] TextWriter stream)
        {
            if (stream != null)
            {
                stream.WriteLine(throwable.ToString());
                stream.WriteLine(throwable.StackTrace);
                stream.Flush();
            }
            else
            {
                Debug.WriteLine(throwable.ToString());
                Debug.WriteLine(throwable.StackTrace);
            }
        }

        ///  <summary>
        /// This method returns a string, representing the content of the given IDictionary.
        /// Returns "null" if parameter is null.
        ///</summary>
        ///<param name="dictionary">
        /// IDictionary to return as string.
        ///</param>
        ///<returns>
        /// The string representation of keys and values in IDictionary.
        ///</returns>
        public static string DictionaryToString(IDictionary dictionary)
        {
            return DictionaryToString(dictionary, true);
        }

        ///  <summary>
        /// This method returns a string, representing the content of the given IDictionary.
        /// Returns "null" if parameter is null.
        ///</summary>
        ///<param name="dictionary">IDictionary to return as string.</param>
        ///<param name="includeTypes"> </param>
        public static string DictionaryToString(IDictionary dictionary, bool includeTypes)
        {
            if (dictionary == null)
            {
                return "null";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (object key in dictionary.Keys)
            {
                Type valueType;
                string value;
                if (sb.Length > 1)
                {
                    sb.Append(", ");
                }
                if (dictionary[key] == null)
                {
                    valueType = typeof(object);
                    value = "null";
                }
                else
                {
                    valueType = dictionary[key].GetType();
                    value = dictionary[key].ToString();
                }
                if ((typeof(IDictionary) == valueType) || (typeof(Hashtable) == valueType))
                {
                    value = DictionaryToString((IDictionary)dictionary[key]);
                }
                if (typeof(string[]) == valueType)
                {
                    value = string.Format("{{{0}}}", string.Join(",", (string[])dictionary[key]));
                }
                if (includeTypes)
                {
                    sb.AppendFormat("({0}){1}=({2}){3}", new object[] { key.GetType().Name, key, valueType.Name, value });
                }
                else
                {
                    sb.AppendFormat("{0}={1}", key, value);
                }
            }
            sb.Append("}");
            return sb.ToString();
        }

        ///<summary>
        /// Inserts the number's value into the byte array, using Big-Endian order (a.k.a. Network-byte-order).
        ///</summary>
        ///<param name="buffer">Byte array to write into.</param>
        ///<param name="index">Index of first position to write to.</param>
        ///<param name="number">Number to write.</param>
        [Obsolete("Use Protocol.Serialize() instead.")]
        public static void NumberToByteArray(byte[] buffer, int index, short number)
        {
            Protocol.Serialize(number, buffer, ref index);
        }

        ///<summary>
        /// Inserts the number's value into the byte array, using Big-Endian order (a.k.a. Network-byte-order).
        ///</summary>
        ///<param name="buffer">Byte array to write into.</param>
        ///<param name="index">Index of first position to write to.</param>
        ///<param name="number">Number to write.</param>
        [Obsolete("Use Protocol.Serialize() instead.")]
        public static void NumberToByteArray(byte[] buffer, int index, int number)
        {
            Protocol.Serialize(number, buffer, ref index);
        }

        ///<summary>
        /// Converts a byte-array to string (useful as debugging output).
        /// Uses BitConverter.ToString(list) internally after a null-check of list.
        ///</summary>
        ///<param name="list">Byte-array to convert to string.</param>
        ///<returns>
        /// List of bytes as string.
        ///</returns>
        public static string ByteArrayToString(byte[] list)
        {
            if (list == null)
            {
                return string.Empty;
            }
            return BitConverter.ToString(list);
        }

        /// <summary>
        /// Class to wrap static access to the random.Next() call in a thread safe manner.
        /// </summary>
        public class ThreadSafeRandom
        {
            private static readonly Random _r = new Random();

            public static int Next()
            {
                lock (_r)
                {
                    return _r.Next();
                }
            }
        }

        public static uint CalculateCrc(byte[] buffer, int length)
        {
            uint crc = uint.MaxValue;
            uint poly = 0xedb88320;
            byte current = 0;
            for (int bufferIndex = 0; bufferIndex < length; bufferIndex++)
            {
                current = buffer[bufferIndex];
                crc ^= current;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 1) != 0)
                    {
                        crc = (crc >> 1) ^ poly;
                    }
                    else
                    {
                        crc = crc >> 1;
                    }
                }
            }
            return crc;
        }

        public static void CallInBackground(Func<bool> myThread)
        {
            Thread x = new Thread(() =>
            {
                while (myThread())
                {
                    Thread.Sleep(100);
                }
            });
            x.IsBackground = true;
            x.Start();
        }

        public static List<MethodInfo> GetMethods(Type type, Type attribute)
        {
            List<MethodInfo> fittingMethods = new List<MethodInfo>();
            if (type != null)
            {
                MethodInfo[] declaredMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (MethodInfo methodInfo in declaredMethods)
                {
                    if ((attribute == null) || methodInfo.IsDefined(attribute, false))
                    {
                        fittingMethods.Add(methodInfo);
                    }
                }
            }
            return fittingMethods;
        }

        [Obsolete("Use DictionaryToString() instead.")]
        public static string HashtableToString(Hashtable hash)
        {
            return DictionaryToString(hash);
        }

        public delegate int IntegerMillisecondsDelegate();
    }
}
