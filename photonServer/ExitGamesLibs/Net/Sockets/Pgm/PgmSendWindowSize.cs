using System;
using System.Runtime.InteropServices;
using ExitGames.Net.Sockets.Pgm.Win32;

namespace ExitGames.Net.Sockets.Pgm
{
    /// <summary>
    /// The RM_SEND_WINDOW structure specifies the Reliable Multicast send window. 
    ///   This structure is used with the RM_RATE_WINDOW_SIZE socket option.
    /// </summary>
    /// <remarks>
    ///           Any combination of the three available members may be set for a given socket option call. 
    ///           For example, one, any two, or all three members may be specified during a setsockopt 
    ///           function call. Regardless of settings, Windows enforces the following ratio: 
    ///          TransmissionRate == (WindowSizeBytes/WindowSizeMSecs) * 8. 
    ///          As such, setting any two parameters effectively sets the third to ensure optimum performance. 
    ///          The combination of these members can affect the resources used on a PGM sender's computer. 
    ///          For example, a large transmission rate value combined with a large window size results in 
    ///          more required buffer space.
    ///</remarks>
    [StructLayout(LayoutKind.Sequential), CLSCompliant(false)]
    public struct PgmSendWindowSize
    {
        /// <summary>
        /// The send window.
        /// </summary>
        private _RM_SEND_WINDOW sendWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSendWindowSize"/> structure.
        /// </summary>
        /// <param name="rateKbitsPerSec">Transmission rate for the send window, in kilobits per second.</param>
        /// <param name="windowSizeInMSecs">The window size for the send window, in milliseconds.</param>
        /// <param name="windowSizeInBytes">The window size for the session, in bytes.</param>
        public PgmSendWindowSize(uint rateKbitsPerSec, uint windowSizeInMSecs, uint windowSizeInBytes)
        {
            sendWindow.RateKbitsPerSec = rateKbitsPerSec;
            sendWindow.WindowSizeInMSecs = windowSizeInMSecs;
            sendWindow.WindowSizeInBytes = windowSizeInBytes;
        }

        /// <summary>
        /// Returns the RM_SEND_WINDOW structure in raw binary format.
        /// </summary>
        /// <returns>Byte array containing RM_SEND_WINDOW structure in raw binary format.</returns>
        public byte[] GetBytes()
        {
            int len = Marshal.SizeOf(sendWindow);
            byte[] buffer = new byte[len];
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(sendWindow, ptr, true);
            Marshal.Copy(ptr, buffer, 0, len);
            Marshal.FreeHGlobal(ptr);
            return buffer;
        }

        /// <summary>
        /// Gets or sets the transmission rate for the send window, 
        ///   in kilobits per second.
        /// </summary>
        public uint RateKbitsPerSec
        {
            get
            {
                return sendWindow.RateKbitsPerSec;
            }
            set
            {
                sendWindow.RateKbitsPerSec = value;
            }
        }

        /// <summary>
        /// Gets or sets the window size for the session, in bytes.
        /// </summary>
        public uint WindowSizeInBytes
        {
            get
            {
                return sendWindow.WindowSizeInBytes;
            }
            set
            {
                sendWindow.WindowSizeInBytes = value;
            }
        }

        /// <summary>
        /// Gets or sets the window size for the send window, in milliseconds.
        /// </summary>
        public uint WindowSizeInMSecs
        {
            get
            {
                return sendWindow.WindowSizeInMSecs;
            }
            set
            {
                sendWindow.WindowSizeInMSecs = value;
            }
        }
    }
}
