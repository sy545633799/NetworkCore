using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PhotonHostRuntimeInterfaces
{
    [ComImport, Guid("7FCFE4AC-1415-46DE-84A2-24FE3AD302A2"), InterfaceType((short)1)]
    public interface ILogToUnmanagedLog
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void LogMessage([In, MarshalAs(UnmanagedType.BStr)] string message);
    }
}
