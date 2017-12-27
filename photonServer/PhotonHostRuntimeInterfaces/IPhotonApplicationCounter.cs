using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PhotonHostRuntimeInterfaces
{
    [ComImport, InterfaceType((short)1), Guid("B8E85EEB-A3DD-4C8E-A0F5-B3C4753B7714")]
    public interface IPhotonApplicationsCounter
    {
        [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        string[] GetConnectionCounts([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] out int[] perApplicationCurrentCount, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] out int[] perApplicationMaxCount, out int currentCount, out int maxCount);
    }
}
