using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ExitGames.Logging;
using ExitGames.Net.Sockets.Pgm.Win32;

namespace ExitGames.Net.Sockets.Pgm
{
    /// <summary>
    /// A socket for PGM.
    /// </summary>
    public sealed class PgmSocket : Socket
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The socket counter.
        /// </summary>
        private static int socketCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSocket"/> class.
        /// </summary>
        public PgmSocket()
            : base(AddressFamily.InterNetwork, SocketType.Rdm, PgmProtocolType)
        {
            Interlocked.Increment(ref socketCounter);
        }

        /// <summary>
        ///             Receiver only. Adds an interface on which to listen (the default is the first local interface enumerated). 
        ///           The value specified replaces the default interface on the first call for a given socket, and adds other interfaces on subsequent calls. 
        ///           To obtain <see cref="F:System.Net.IPAddress.Any"/> behavior, each network interface must be added separately.
        /// </summary>
        /// <param name="interfaceIpAddress">IPAdress of the network interface to add.</param>
        public void AddReceiveInterface(IPAddress interfaceIpAddress)
        {
            byte[] buffer = interfaceIpAddress.GetAddressBytes();
            SetPgmSocketOption(PgmSocketOptions.RM_ADD_RECEIVE_IF, buffer);
        }

        /// <summary>
        /// Receiver only. Adds an list of interfaces on which to listen.
        /// </summary>
        /// <param name="interfaceIpAddresses">IPAdress list of the network interfaces to add.</param>
        /// <seealso cref="M:ExitGames.Net.Sockets.Pgm.PgmSocket.AddReceiveInterface(System.Net.IPAddress)"/>
        public void AddReceiveInterfaces(IEnumerable<IPAddress> interfaceIpAddresses)
        {
            foreach (IPAddress ipAddress in interfaceIpAddresses)
            {
                AddReceiveInterface(ipAddress);
            }
        }

        /// <summary>
        ///  Receiver only. Specifies whether a high bandwidth LAN (100Mbps+) connection is used.
        /// </summary>
        /// <param name="value">True enables high speed.</param>
        public void SetHighSpeedIntranetOption(bool value)
        {
            SetPgmSocketOption(PgmSocketOptions.RM_HIGH_SPEED_INTRANET_OPT, value ? 1 : 0);
        }

        /// <summary>
        /// Specifies if the socket is allowed to be bound to an address that is already in use.
        /// </summary>
        /// <param name="value">True to allow multiple sockets on one address.</param>
        public void SetReuseAddress(bool value)
        {
            SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, value);
            if (log.IsDebugEnabled)
            {
                LogSetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, value);
            }
        }

        /// <summary>
        /// Sender only. Sets the sending interface IP address.
        /// </summary>
        /// <param name="interfaceIpAddress">IPAdress of the network interface.</param>
        public void SetSendInterface(IPAddress interfaceIpAddress)
        {
            byte[] buffer = interfaceIpAddress.GetAddressBytes();
            SetPgmSocketOption(PgmSocketOptions.RM_SET_SEND_IF, buffer);
        }

        /// <summary>
        /// Sets a new send window size.
        /// </summary>
        /// <param name="sendWindowSize"> The send window size.</param>
        [CLSCompliant(false)]
        public void SetSendWindowSize(PgmSendWindowSize sendWindowSize)
        {
            byte[] bytes = sendWindowSize.GetBytes();
            SetPgmSocketOption(PgmSocketOptions.RM_RATE_WINDOW_SIZE, bytes);
        }

        /// <summary>
        /// Sets pgm socket option.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="value">The value.</param>
        internal void SetPgmSocketOption(PgmSocketOptions option, int value)
        {
            try
            {
                SetSocketOption(PgmSocketOptionLevel, (SocketOptionName)option, value);
                if (log.IsDebugEnabled)
                {
                    LogSetSocketOption(PgmSocketOptionLevel, option, value);
                }
            }
            catch (SocketException exception)
            {
                LogSetSocketOptionException(PgmSocketOptionLevel, option, value, exception);
            }
        }

        /// <summary>
        /// Sets pgm socket option.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="value">The value.</param>
        internal void SetPgmSocketOption(PgmSocketOptions option, byte[] value)
        {
            try
            {
                SetSocketOption(PgmSocketOptionLevel, (SocketOptionName)option, value);
                if (log.IsDebugEnabled)
                {
                    LogSetSocketOption(PgmSocketOptionLevel, option, value);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled)
                {
                    LogSetSocketOptionException(PgmSocketOptionLevel, option, value, exception);
                }
            }
        }

        /// <summary>
        /// Byte array to hex string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The byte array to hex string.</returns>
        private static string ByteArrayToHexString(byte[] value)
        {
            StringBuilder builder = new StringBuilder(value.Length * 2 + 2);
            builder.Append("0x");
            for (int i = 0; i < value.Length; i++)
            {
                builder.Append(value[i].ToString("X2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// The log set socket option.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="option">The option.</param>
        /// <param name="value">The value.</param>
        private static void LogSetSocketOption(SocketOptionLevel level, object option, object value)
        {
            log.DebugFormat("Set socket option: level={0}; option={1}; value={2}", level, option, value);
        }

        /// <summary>
        ///  The log set socket option.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="option">The option.</param>
        /// <param name="value">The value.</param>
        private static void LogSetSocketOption(SocketOptionLevel level, object option, byte[] value)
        {
            log.DebugFormat("Set socket option: level={0}; option={1}; value={2}", level, option, ByteArrayToHexString(value));
        }

        /// <summary>
        ///  The log set socket option exception.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="option">The option.</param>
        /// <param name="value">The value.</param>
        /// <param name="exception">The exception.</param>
        private static void LogSetSocketOptionException(SocketOptionLevel level, object option, object value, Exception exception)
        {
            log.Error(string.Format("Set socket option: level={0}; option={1}; value={2}", level, option, value), exception);
        }

        /// <summary>
        /// The log set socket option exception.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="option">The option.</param>
        /// <param name="value">The value.</param>
        /// <param name="exception">The exception.</param>
        private static void LogSetSocketOptionException(SocketOptionLevel level, object option, byte[] value, Exception exception)
        {
            log.Error(string.Format("Set socket option: level={0}; option={1}; value={2}", level, option, ByteArrayToHexString(value), exception));
        }

        /// <summary>
        ///  Gets the <see cref="T:System.Net.Sockets.SocketOptionLevel"/> used to set 
        ///   Pragmatic General Multicast (PGM) protocol specific socket options.
        /// </summary>
        /// <value>Equals <see cref="F:ExitGames.Net.Sockets.Pgm.Win32.PgmConstants.IpProtoRm"/>.</value>
        public static ProtocolType PgmProtocolType
        {
            get
            {
                return (ProtocolType)0x71;
            }
        }

        /// <summary>
        /// Gets the <see cref="T:System.Net.Sockets.SocketOptionLevel"/> used to set 
        ///  Pragmatic General Multicast (PGM) protocol specific socket options.
        /// </summary>
        /// <value>Equals <see cref="F:ExitGames.Net.Sockets.Pgm.Win32.PgmConstants.IpProtoRm"/>.</value>
        public static SocketOptionLevel PgmSocketOptionLevel
        {
            get
            {
                return (SocketOptionLevel)0x71;
            }
        }
    }
}
