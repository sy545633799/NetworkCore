using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PhotonHostRuntimeInterfaces
{
    [ComImport, Guid("84600867-4FB2-4B71-8BE5-1166B59E1024"), InterfaceType((short)1)]
    public interface IUnloadApplicationDomains
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void UnloadAppDomain([In] int applicationDomainID, [In, MarshalAs(UnmanagedType.BStr)] string reason);
    }
}
