using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PhotonHostRuntimeInterfaces
{
    [ComImport, Guid("D206406C-126A-4396-8C5D-EB52EBA79739"), InterfaceType((short)1)]
    public interface IPhotonPeer
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetConnectionID();
        [return: ComAliasName("PhotonHostRuntimeInterfaces.SendResults")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        SendResults Send([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] byte[] data, [In, ComAliasName("PhotonHostRuntimeInterfaces.MessageReliablity")] MessageReliablity reliability, [In] byte channelId, [In, ComAliasName("PhotonHostRuntimeInterfaces.MessageContentType")] MessageContentType MessageContentType);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Flush();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void DisconnectClient();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void AbortClient();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        ushort GetLocalPort();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        string GetLocalIP();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        ushort GetRemotePort();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        string GetRemoteIP();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetUserData([In, MarshalAs(UnmanagedType.IUnknown)] object pObj);
        [return: MarshalAs(UnmanagedType.IUnknown)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        object GetUserData();
        [return: ComAliasName("PhotonHostRuntimeInterfaces.ListenerType")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        ListenerType GetListenerType();
        [return: ComAliasName("PhotonHostRuntimeInterfaces.PeerType")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        PeerType GetPeerType();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetDebugString([In, MarshalAs(UnmanagedType.BStr)] string debugString);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetStats(out int rtt, out int rttVariance, out int numFailures);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x40)]
        IntPtr _InternalGetPeerInfo([In] int why);
        [return: ComAliasName("PhotonHostRuntimeInterfaces.SendResults")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x40)]
        SendResults _InternalBroadcastSend([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] byte[] data, [In, ComAliasName("PhotonHostRuntimeInterfaces.MessageReliablity")] MessageReliablity reliability, [In] byte channelId, [In, ComAliasName("PhotonHostRuntimeInterfaces.MessageContentType")] MessageContentType MessageContentType);
    }

 

 

}
