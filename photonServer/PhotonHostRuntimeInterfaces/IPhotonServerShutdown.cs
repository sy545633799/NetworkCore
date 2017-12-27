using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PhotonHostRuntimeInterfaces
{
    [ComImport, Guid("29549CCE-45B6-4CA7-ADC0-79E9C065BEFA"), InterfaceType((short)1)]
    public interface IPhotonServerShutdown
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Shutdown([In, MarshalAs(UnmanagedType.BStr)] string reason);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ShutdownDueToError([In, MarshalAs(UnmanagedType.BStr)] string reason, [In] int exitCode);
    }
}
