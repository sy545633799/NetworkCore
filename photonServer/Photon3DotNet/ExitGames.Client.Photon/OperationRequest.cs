namespace ExitGames.Client.Photon
{
    using System.Collections.Generic;

    ///<summary>
    /// Container for an Operation request, which is a code and parameters.
    ///</summary>
    ///<remarks>
    /// On the lowest level, Photon only allows byte-typed keys for operation parameters.
    /// The values of each such parameter can be any serializable datatype: byte, int, hashtable and many more.
    ///</remarks>
    public class OperationRequest
    {
        /// <summary>
        /// Byte-typed code for an operation - the short identifier for the server's method to call.
        /// </summary>
        public byte OperationCode;

        /// <summary>
        /// The parameters of the operation - each identified by a byte-typed code in Photon.
        /// </summary>
        public Dictionary<byte, object> Parameters;
    }
}
