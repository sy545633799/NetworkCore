using System;
using System.Collections.Generic;

namespace Photon.SocketServer.Rpc.Protocols
{
    internal class CustomTypeCache
    {
        // Fields
        private static readonly Dictionary<byte, CustomTypeInfo> codeDict = new Dictionary<byte, CustomTypeInfo>();
        private static readonly Dictionary<Type, CustomTypeInfo> typeDict = new Dictionary<Type, CustomTypeInfo>();

        // Methods
        internal static void Clear()
        {
            codeDict.Clear();
            typeDict.Clear();
        }

        internal static bool TryGet(byte typeCode, out CustomTypeInfo customTypeInfo)
        {
            return codeDict.TryGetValue(typeCode, out customTypeInfo);
        }

        internal static bool TryGet(Type type, out CustomTypeInfo customTypeInfo)
        {
            return typeDict.TryGetValue(type, out customTypeInfo);
        }

        internal static bool TryRegisterType(Type type, byte typeCode, Func<object, byte[]> serializeFunction, Func<byte[], object> deserializeFunction)
        {
            if (codeDict.ContainsKey(typeCode) || typeDict.ContainsKey(type))
            {
                return false;
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (serializeFunction == null)
            {
                throw new ArgumentNullException("serializeFunction");
            }
            if (deserializeFunction == null)
            {
                throw new ArgumentNullException("deserializeFunction");
            }
            CustomTypeInfo info = new CustomTypeInfo(type, typeCode, serializeFunction, deserializeFunction);
            codeDict.Add(typeCode, info);
            typeDict.Add(type, info);
            return true;
        }
    }
}
