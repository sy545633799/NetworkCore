using System;

namespace Photon.SocketServer.ServerToServer
{
    /// <summary>
    ///  Contains information for a intiialize encryption operation response.
    /// </summary>
    public class InitializeEncryptionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.InitializeEncryptionEventArgs"/> class.
        /// </summary>
        /// <param name="returnCode">The received return code.</param>
        /// <param name="debugMessage">The received debug message.</param>
        public InitializeEncryptionEventArgs(short returnCode, string debugMessage)
        {
            this.ReturnCode = returnCode;
            this.DebugMessage = debugMessage;
        }

        /// <summary>
        /// Gets a the debeug message of the initialize encryption response.
        /// </summary>
        public string DebugMessage { get; private set; }

        /// <summary>
        /// Gets the return code from the initialize enryption response.
        /// </summary>
        public short ReturnCode { get; private set; }
    }
}
