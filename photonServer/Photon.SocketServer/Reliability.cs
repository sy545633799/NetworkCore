namespace Photon.SocketServer
{
    /// <summary>
    /// Determines if an <see cref="T:Photon.SocketServer.OperationRequest"/>, an <see
    /// cref="T:Photon.SocketServer.OperationResponse"/> or an <see 
    /// cref="T:Photon.SocketServer.EventData"/> is transported reliable or unreliable.
    /// </summary>
    public enum Reliability
    {
        /// <summary>
        /// Reliable events/operations are guaranteed to arrive unless the client disconnects.
        /// </summary>
        Reliable = 3,

        /// <summary>
        ///  Unreliable events/operations are not guaranteed to arrive.
        /// </summary>
        Unreliable = 2
    }
}
