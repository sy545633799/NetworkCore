using Photon.SocketServer.Rpc.Protocols.Amf3;
using Photon.SocketServer.Rpc.Reflection;

namespace Photon.SocketServer.Rpc.Protocols.GpBinaryByte
{
    /// <summary>
    /// TODO: For testing only
    /// </summary>
    internal class GpBinaryByteProtocolV16Flash : GpBinaryByteProtocolV16
    {
        // Fields
        public static readonly GpBinaryByteProtocolV16Flash FlashHeaderV2Instance = new GpBinaryByteProtocolV16Flash(ProtocolType.GpBinaryV162, RtsMessageHeaderConverterBinaryV2.Instance);

        // Methods
        protected GpBinaryByteProtocolV16Flash(ProtocolType protocolType, IRtsMessageHeaderConverter headerWriter)
            : base(protocolType, headerWriter)
        {
        }

        public override bool TryConvertParameter(ObjectMemberInfo<DataMemberAttribute> paramterInfo, ref object value)
        {
            return Amf3Protocol.HeaderV2Instance.TryConvertParameter(paramterInfo, ref value);
        }
    }
}
