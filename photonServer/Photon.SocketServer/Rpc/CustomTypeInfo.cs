using System;

namespace Photon.SocketServer.Rpc
{
    internal class CustomTypeInfo
    {
        // Fields
        public readonly byte Code;
        public readonly Func<byte[], object> DeserializeFunction;
        public readonly Func<object, byte[]> SerializeFunction;
        public readonly Type Type;

        // Methods
        public CustomTypeInfo(Type type, byte code, Func<object, byte[]> serializeFunction, Func<byte[], object> deserializeFunction)
        {
            this.Type = type;
            this.Code = code;
            this.SerializeFunction = serializeFunction;
            this.DeserializeFunction = deserializeFunction;
        }
    }
}
