using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PhotonHostRuntimeInterfaces
{
    [ComImport, InterfaceType((short)1), Guid("B6030194-4E64-4081-A718-0334767CA974")]
    public interface IControlListeners
    {
        [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        string[] GetListeners([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] out ListenerStatus[] statuses);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        bool StopListener([In, MarshalAs(UnmanagedType.BStr)] string listener);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        bool StartListener([In, MarshalAs(UnmanagedType.BStr)] string listener);
    }
}
