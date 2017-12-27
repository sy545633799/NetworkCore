using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PhotonHostRuntimeInterfaces
{
    [ComImport, Guid("C2FD6E8B-8F83-4EF8-B57A-FBE2FC161FAF"), InterfaceType((short)1)]
    public interface IPhotonDomainManager
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void InitialiseDefaultAppDomain([In, ComAliasName("PhotonHostRuntimeInterfaces.UnhandledExceptionPolicy")] UnhandledExceptionPolicy UnhandledExceptionPolicy, [In, MarshalAs(UnmanagedType.Interface)] IUnloadApplicationDomains appDomainUnloader, [In, MarshalAs(UnmanagedType.Interface)] ILogToUnmanagedLog log, [In, MarshalAs(UnmanagedType.BStr)] string unmanagedLogDirectory, [In, MarshalAs(UnmanagedType.Interface)] IControlListeners listenerControl, [In, MarshalAs(UnmanagedType.Interface)] IManageClientConnectionCount connectionCounter);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        bool GetLicenseInformation([In, MarshalAs(UnmanagedType.Interface)] IPhotonServerShutdown serverShutdown, out int maxConcurrentConnections, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] validIPs);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int CreateAppDomain([In, MarshalAs(UnmanagedType.BStr)] string name, [In, MarshalAs(UnmanagedType.BStr)] string assemblyName, [In, MarshalAs(UnmanagedType.BStr)] string baseDirectory, [In, MarshalAs(UnmanagedType.BStr)] string applicationRootDirectory, [In, MarshalAs(UnmanagedType.BStr)] string applicationDirectory, [In, MarshalAs(UnmanagedType.BStr)] string sharedDirectory, [In, MarshalAs(UnmanagedType.BStr)] string applicationSharedDirectory, [In] bool enableShadowCopy, [In] bool createPerInstanceShadowCopyCaches, [In, MarshalAs(UnmanagedType.BStr)] string instanceName);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        IPhotonApplication Start([In, MarshalAs(UnmanagedType.BStr)] string assemblyName, [In, MarshalAs(UnmanagedType.BStr)] string typeName, [In, MarshalAs(UnmanagedType.BStr)] string instanceName, [In, MarshalAs(UnmanagedType.BStr)] string applicationName, [In, MarshalAs(UnmanagedType.Interface)] IPhotonApplicationSink sink, [In, MarshalAs(UnmanagedType.Interface)] ILogToUnmanagedLog log, [In, MarshalAs(UnmanagedType.Interface)] IControlListeners listenerControl);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void PhotonRunning();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void RequestStop();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Stop();
    }
}
