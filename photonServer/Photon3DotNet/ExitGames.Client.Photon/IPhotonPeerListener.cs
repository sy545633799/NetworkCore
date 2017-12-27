namespace ExitGames.Client.Photon
{
    ///<summary>
    /// Callback interface for the Photon client side. Must be provided to a new PhotonPeer in its constructor.
    ///</summary>
    ///<remarks>
    /// These methods are used by your PhotonPeer instance to keep your app updated. Read each method's
    /// description and check out the samples to see how to use them.
    ///</remarks>
    public interface IPhotonPeerListener
    {
        ///<summary>
        /// Provides textual descriptions for various error conditions and noteworthy situations.
        /// In cases where the application needs to react, a call to OnStatusChanged is used.
        /// OnStatusChanged gives "feedback" to the game, DebugReturn provies human readable messages 
        /// on the background.
        ///</summary>
        ///<remarks>
        /// All debug output of the library will be reported through this method. Print it or put it in a 
        /// buffer to use it on-screen. Use PhotonPeer.DebugOut to select how verbose the output is.
        ///</remarks>
        ///<param name="level">DebugLevel (severity) of the message.</param>
        ///<param name="message">Debug text. Print to System.Console or screen.</param>
        void DebugReturn(DebugLevel level, string message);

        ///<summary>
        /// Called whenever an event from the Photon Server is dispatched.
        ///</summary>
        ///<remarks>
        /// Events are used for communication between clients and allow the server to update clients over time. 
        /// The creation of an event is often triggered by an operation (called by this client or an other). 

        /// Each event carries its specific content in its Parameters. Your application knows which content to
        /// expect by checking the event's 'type', given by the event's Code.

        /// Events can be defined and extended server-side. 

        /// If you use the Lite application as base, several events like EvJoin and EvLeave are already defined.
        /// For these events and their Parameters, the library provides constants, so check:
        /// LiteEventCode and LiteEventKey classes.
        /// Lite also allows you to come up with custom events on the fly, purely client-side. To do so, use 
        /// LitePeer.OpRaiseEvent.<para></para>

        /// Events are buffered on the client side and must be Dispatched. This way, OnEvent is always taking
        /// place in the same thread as a <see cref="M:ExitGames.Client.Photon.PhotonPeer.DispatchIncomingCommands"/> call.
        ///</remarks>
        ///<param name="eventData">The event currently being dispatched.</param>
        void OnEvent(EventData eventData);

        ///<summary>
        /// Callback method which gives you (async) responses for called operations.
        ///</summary>
        ///<remarks>
        /// Like methods, operations can have a result. As operation-calls are non-blocking, the response
        /// for any operation of this peer is provided by this method. 
        /// As example: Joining a room on a Lite-based server will return a list of players currently in gamey
        /// your actorNumber and some other values.

        /// This method is used as general callback for all operations. Each response corresponds to a certain 
        /// "type" of operation by its OperationCode (see: <see cref="!:Operations"/>).<para></para>

        /// The "Lite Application" uses these OpCodes:
        /// * <see cref="F:ExitGames.Client.Photon.Lite.LiteOpCode.Join"/> for OpJoin, contains the actorNr of "this" player
        /// * <see cref="F:ExitGames.Client.Photon.Lite.LiteOpCode.Leave"/> when leaving a room
        /// * <see cref="F:ExitGames.Client.Photon.Lite.LiteOpCode.RaiseEvent"/> for OpRaiseEvent, if this was sent as reliable command
        /// * <see cref="F:ExitGames.Client.Photon.Lite.LiteOpCode.SetProperties"/> for OpSetPropertiesOfActor and OpSetPropertiesOfGame
        ///</remarks>
        ///<example>
        /// When you join a room, the server will assign a consecutive number to each client: the 
        /// "actorNr" or "player number". This is sent back in the OperationResult's 
        /// Parameters as value of key <see cref="F:ExitGames.Client.Photon.Lite.LiteEventKey.ActorNr"/>.<para></para>

        /// Fetch your actorNr of a Join response like this:<para></para>
        ///<c>int actorNr = (int)operationResponse[(byte)LiteOpKey.ActorNr];</c>
        ///</example>
        ///<param name="operationResponse">The response to an operation\-call.</param>  
        void OnOperationResponse(OperationResponse operationResponse);

        ///<summary>
        /// OnStatusChanged is called to let the game know when asyncronous actions finished or when errors happen.
        ///</summary>
        ///<remarks>
        /// Not all of the many StatusCode values will apply to your game. Example: If you don't use encryption, 
        /// the respective status changes are never made.

        /// The values are all part of the StatusCode enumeration and described value-by-value.
        ///</remarks>
        ///<param name="statusCode">A code to identify the situation.</param>
        void OnStatusChanged(StatusCode statusCode);
    }
}
