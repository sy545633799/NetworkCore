using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PhotonHostRuntimeInterfaces
{
    [ComImport, InterfaceType((short)1), Guid("674ACE1E-03B8-4984-A7F3-6B7D26A9F8D4")]
    public interface IManageClientConnectionCount : IPhotonApplicationsCounter
    {
        [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        string[] GetConnectionCounts([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] out int[] perApplicationCurrentCount, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] out int[] perApplicationMaxCount, out int currentCount, out int maxCount);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ResetMaxCounts([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] string[] applicationNames, [In] bool resetTotalCount);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void resetTotalCount();
    }
}
