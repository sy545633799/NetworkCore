using System;
using System.Runtime.InteropServices;

namespace PhotonControl.Helper
{
    public class Wow64RedirectionDisabler : IDisposable
    {
        // Fields
        private readonly IntPtr oldValue;

        // Methods
        public Wow64RedirectionDisabler()
        {
            try
            {
                this.Success = Wow64DisableWow64FsRedirection(out this.oldValue);
            }
            catch (Exception)
            {
            }
        }

        public void Dispose()
        {
            try
            {
                if (this.Success)
                {
                    this.Success = Wow64RevertWow64FsRedirection(this.oldValue);
                }
            }
            catch (Exception)
            {
            }
        }

        [DllImport("Kernel32")]
        private static extern bool Wow64DisableWow64FsRedirection(out IntPtr oldValue);
        [DllImport("Kernel32")]
        private static extern bool Wow64RevertWow64FsRedirection(IntPtr oldValue);

        // Properties
        public bool Success { get; private set; }
    }
}
