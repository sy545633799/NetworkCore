using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PhotonHostRuntimeInterfaces
{
    [ComImport, Guid("217123B5-CAB5-4B54-89CE-F5C20399C049"), InterfaceType((short)1)]
    public interface IPhotonApplication
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnInit([In, MarshalAs(UnmanagedType.Interface)] IPhotonPeer peer, [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] byte[] data, [In] byte channelCount);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnReceive([In, MarshalAs(UnmanagedType.Interface)] IPhotonPeer peer, [In, MarshalAs(UnmanagedType.IUnknown)] object userData, [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] byte[] data, [In, ComAliasName("PhotonHostRuntimeInterfaces.MessageReliablity")] MessageReliablity reliability, [In] byte channelId, [In, ComAliasName("PhotonHostRuntimeInterfaces.MessageContentType")] MessageContentType MessageContentType, [In] int rtt, [In] int rttVariance, [In] int numFailures);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnDisconnect([In, MarshalAs(UnmanagedType.Interface)] IPhotonPeer peer, [In, MarshalAs(UnmanagedType.IUnknown)] object userData, [In, ComAliasName("PhotonHostRuntimeInterfaces.DisconnectReason")] DisconnectReason reasonCode, [In, MarshalAs(UnmanagedType.BStr)] string reasonDetail, [In] int rtt, [In] int rttVariance, [In] int numFailures);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnOutboundConnectionEstablished([In, MarshalAs(UnmanagedType.Interface)] IPhotonPeer peer, [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UI1)] byte[] data, [In, MarshalAs(UnmanagedType.IUnknown)] object userData);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnOutboundConnectionFailed([In, MarshalAs(UnmanagedType.Interface)] IPhotonPeer peer, [In, MarshalAs(UnmanagedType.IUnknown)] object userData, [In] int errorCode, [In, MarshalAs(UnmanagedType.BStr)] string errorMessage);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnFlowControlEvent([In, MarshalAs(UnmanagedType.Interface)] IPhotonPeer peer, [In, MarshalAs(UnmanagedType.IUnknown)] object userData, [In, ComAliasName("PhotonHostRuntimeInterfaces.FlowControlEvent")] FlowControlEvent FlowControlEvent);
    }

}
