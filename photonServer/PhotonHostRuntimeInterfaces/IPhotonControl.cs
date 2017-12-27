using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PhotonHostRuntimeInterfaces
{
    [ComImport, Guid("A9677377-E277-49F2-9573-2F4A1E48129F"), InterfaceType((short)1)]
    public interface IPhotonControl
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnPhotonRunning();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        IPhotonApplication OnStart([In, MarshalAs(UnmanagedType.BStr)] string instanceName, [In, MarshalAs(UnmanagedType.BStr)] string applicationName, [In, MarshalAs(UnmanagedType.Interface)] IPhotonApplicationSink sink, [In, MarshalAs(UnmanagedType.Interface)] IControlListeners listenerControl, [In, MarshalAs(UnmanagedType.Interface)] IPhotonApplicationsCounter applicationsCounter, [In, MarshalAs(UnmanagedType.BStr)] string unmanagedLogDirectory);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnStopRequested();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnStop();
    }
}
