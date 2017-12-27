using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PhotonHostRuntimeInterfaces
{
    [ComImport, Guid("C2D102A3-F3DA-4DEC-8A25-3B7308310CA3"), InterfaceType((short)1)]
    public interface IPhotonApplicationSink
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        bool BroadcastEvent([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN)] IPhotonPeer[] peerList, [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] byte[] data, [In, ComAliasName("PhotonHostRuntimeInterfaces.MessageReliablity")] MessageReliablity reliability, [In] byte channelId, [In, ComAliasName("PhotonHostRuntimeInterfaces.MessageContentType")] MessageContentType MessageContentType, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] out SendResults[] results);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        IPhotonPeer Connect([In, MarshalAs(UnmanagedType.BStr)] string ipAddress, [In] ushort port, [In, Optional, MarshalAs(UnmanagedType.IUnknown)] object userData);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        IPhotonPeer ConnectMux([In, MarshalAs(UnmanagedType.BStr)] string ipAddress, [In] ushort port, [In, Optional, MarshalAs(UnmanagedType.IUnknown)] object userData);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        IPhotonPeer ConnectENet([In, MarshalAs(UnmanagedType.BStr)] string ipAddress, [In] ushort port, [In] byte channelCount, [In, Optional, MarshalAs(UnmanagedType.IUnknown)] object userData, [In, Optional, MarshalAs(UnmanagedType.Struct)] object mtu);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        IPhotonPeer ConnectHixie76WebSocket([In, MarshalAs(UnmanagedType.BStr)] string ipAddress, [In] ushort port, [In, MarshalAs(UnmanagedType.BStr)] string url, [In, MarshalAs(UnmanagedType.BStr)] string origin, [In, Optional, MarshalAs(UnmanagedType.IUnknown)] object userData);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        IPhotonPeer ConnectWebSocket([In, MarshalAs(UnmanagedType.BStr)] string ipAddress, [In] ushort port, [In, ComAliasName("PhotonHostRuntimeInterfaces.WebSocketVersion")] WebSocketVersion version, [In, MarshalAs(UnmanagedType.BStr)] string url, [In, MarshalAs(UnmanagedType.BStr)] string subProtocols, [In, Optional, MarshalAs(UnmanagedType.IUnknown)] object userData);
    }
}
