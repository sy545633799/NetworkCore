namespace ExitGames.Client.Photon.Lite
{
    using System.Collections;
    using System.Collections.Generic;

    ///<summary>
    /// A LitePeer is an extended PhotonPeer and implements the operations offered by the "Lite" Application
    /// of the Photon Server SDK.
    /// </summary>
    /// <remarks>
    /// This class is used by our samples and allows rapid development of simple games. You can use rooms and
    /// properties and send events. For many games, this is a good start.

    /// Operations are prefixed as "Op" and are always asynchronous. In most cases, an OperationResult is
    /// provided by a later call to OnOperationResult.
    /// </remarks>
    public class LitePeer : PhotonPeer
    {
        ///<summary>
        /// Creates a LitePeer instance to connect and communicate with a Photon server.<para></para>
        /// Uses UDP as protocol (except in the Silverlight library).
        ///</summary>
        protected LitePeer()
            : base(ConnectionProtocol.Udp)
        {
        }

        /// <summary>
        /// Creates a LitePeer instance to connect and communicate with a Photon server.
        /// </summary>
        /// <param name="protocolType"></param>
        protected LitePeer(ConnectionProtocol protocolType)
            : base(protocolType)
        {
        }

        ///<summary>
        /// Creates a LitePeer instance to connect and communicate with a Photon server.<para></para>
        /// Uses UDP as protocol (except in the Silverlight library).
        /// </summary>
        /// <param name="listener">Your IPhotonPeerListener implementation.</param>
        public LitePeer(IPhotonPeerListener listener)
            : base(listener, ConnectionProtocol.Udp)
        {
        }

        ///<summary>
        /// Creates a LitePeer instance to communicate with Photon with your selection of protocol.
        /// We recommend UDP.
        ///</summary>
        ///<param name="listener">Your IPhotonPeerListener implementation.</param>
        ///<param name="protocolType">Protocol to use to connect to Photon.</param>
        public LitePeer(IPhotonPeerListener listener, ConnectionProtocol protocolType)
            : base(listener, protocolType)
        {
        }

        ///<summary>
        /// Operation to handle this client's interest groups (for events in room).
        ///</summary>
        ///<remarks>
        /// Note the difference between passing null and byte[0]:
        ///  null won't add/remove any groups.
        ///  byte[0] will add/remove all (existing) groups.
        /// First, removing groups is executed. This way, you could leave all groups and join only the ones provided.
        ///</remarks>
        ///<param name="groupsToRemove">Groups to remove from interest. Null will not leave any. A byte[0] will remove all.</param>
        ///<param name="groupsToAdd">Groups to add to interest. Null will not add any. A byte[0] will add all current.</param>
        ///<returns></returns>
        public virtual bool OpChangeGroups(byte[] groupsToRemove, byte[] groupsToAdd)
        {
            if (base.DebugOut >= DebugLevel.ALL)
            {
                base.Listener.DebugReturn(DebugLevel.ALL, "OpChangeGroups()");
            }
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            if (groupsToRemove != null)
            {
                opParameters[0xef] = groupsToRemove;
            }
            if (groupsToAdd != null)
            {
                opParameters[0xee] = groupsToAdd;
            }
            return this.OpCustom(0xf8, opParameters, true, 0);
        }

        ///<summary>
        /// Gets all properties of the game and each actor.
        ///</summary>
        ///<remarks>
        /// Please read the general description of <see cref="!:Properties on Photon"/>.
        ///</remarks>
        ///<param name="channelId">Number of channel to use (starting with 0).</param>
        ///<returns>If operation could be enqueued for sending</returns>
        public virtual bool OpGetProperties(byte channelId)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(0xfb, (byte)3);
            return this.OpCustom(0xfb, opParameters, true, channelId);
        }

        ///<summary>
        /// Gets selected properties of some actors.
        ///</summary>
        ///<remarks>
        /// Please read the general description of <see cref="!:Properties on Photon"/>.
        ///</remarks>
        ///<param name="properties">array of property keys to fetch. optional (can be null).</param>
        ///<param name="actorNrList">optional, a list of actornumbers to get the properties of</param>
        ///<param name="channelId">Number of channel to use (starting with 0).</param>
        ///<returns>If operation could be enqueued for sending</returns>
        public virtual bool OpGetPropertiesOfActor(int[] actorNrList, byte[] properties, byte channelId)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(0xfb, LitePropertyTypes.Actor);
            if (properties != null)
            {
                opParameters.Add(0xf9, properties);
            }
            if (actorNrList != null)
            {
                opParameters.Add(0xfc, actorNrList);
            }
            return this.OpCustom(0xfb, opParameters, true, channelId);
        }

        ///<summary>
        /// Gets selected properties of an actor.
        ///</summary>
        ///<remarks>
        /// Please read the general description of <see cref="!:Properties on Photon"/>.
        ///</remarks>
        ///<param name="properties">optional, array of property keys to fetch</param>
        ///<param name="actorNrList">optional, a list of actornumbers to get the properties of</param>
        ///<param name="channelId">Number of channel to use (starting with 0).</param>
        ///<returns>If operation could be enqueued for sending</returns>
        public virtual bool OpGetPropertiesOfActor(int[] actorNrList, string[] properties, byte channelId)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(0xfb, LitePropertyTypes.Actor);
            if (properties != null)
            {
                opParameters.Add(0xf9, properties);
            }
            if (actorNrList != null)
            {
                opParameters.Add(0xfc, actorNrList);
            }
            return this.OpCustom(0xfb, opParameters, true, channelId);
        }

        ///<summary>
        /// Gets selected properties of current game.
        ///</summary>
        ///<remarks>
        /// Please read the general description of <see cref="!:Properties on Photon"/>.
        ///</remarks>
        ///<param name="properties">array of property keys to fetch. optional (can be null).</param>
        ///<param name="channelId">Number of channel to use (starting with 0).</param>
        ///<returns>If operation could be enqueued for sending</returns>
        public virtual bool OpGetPropertiesOfGame(byte[] properties, byte channelId)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(0xfb, LitePropertyTypes.Game);
            if (properties != null)
            {
                opParameters.Add(0xf8, properties);
            }
            return this.OpCustom(0xfb, opParameters, true, channelId);
        }

        ///<summary>
        /// Gets selected properties of current game.
        ///</summary>
        ///<remarks>
        /// Please read the general description of <see cref="!:Properties on Photon"/>.
        ///</remarks>
        ///<param name="properties">array of property keys to fetch. optional (can be null).</param>
        ///<param name="channelId">Number of channel to use (starting with 0).</param>
        ///<returns>If operation could be enqueued for sending</returns>
        public virtual bool OpGetPropertiesOfGame(string[] properties, byte channelId)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(0xfb, LitePropertyTypes.Game);
            if (properties != null)
            {
                opParameters.Add(0xf8, properties);
            }
            return this.OpCustom(0xfb, opParameters, true, channelId);
        }

        ///<summary>
        /// This operation will join an existing room by name or create one if the name is not in use yet.

        /// Rooms (or games) are simply identified by name. We assume that users always want to get into a room - no matter
        /// if it existed before or not, so it might be a new one. If you want to make sure a room is created (new, empty),
        /// the client side might come up with a unique name for it (make sure the name was not taken yet).

        /// The application "Lite Lobby" lists room names and effectively allows the user to select a distinct one.

        /// Each actor (a.k.a. player) in a room will get events that are raised for the room by any player.

        /// To distinguish the actors, each gets a consecutive actornumber. This is used in events to mark who triggered
        /// the event. A client finds out it's own actornumber in the return callback for operation Join. Number 1 is the
        /// lowest actornumber in each room and the client with that actornumber created the room.

        /// Each client could easily send custom data around. If the data should be available to newcomers, it makes sense
        /// to use Properties.

        /// Joining a room will trigger the event <see cref="F:ExitGames.Client.Photon.Lite.LiteEventCode.Join" text="LiteEventCode.Join"/>, which contains
        /// the list of actorNumbers of current players inside the  room
        /// (<see cref="F:ExitGames.Client.Photon.Lite.LiteEventKey.ActorList" text="LiteEventKey.ActorList"/>). This also gives you a count of current
        /// players.
        /// </summary>
        /// <param name="gameName">Any identifying name for a room / game.</param>
        /// <returns>If operation could be enqueued for sending</returns>
        public virtual bool OpJoin(string gameName)
        {
            return this.OpJoin(gameName, null, null, false);
        }

        ///<summary>
        /// This operation will join an existing room by name or create one if the name is not in use yet.

        /// Rooms (or games) are simply identified by name. We assume that users always want to get into a room - no matter
        /// if it existed before or not, so it might be a new one. If you want to make sure a room is created (new, empty),
        /// the client side might come up with a unique name for it (make sure the name was not taken yet).

        /// The application "Lite Lobby" lists room names and effectively allows the user to select a distinct one.

        /// Each actor (a.k.a. player) in a room will get events that are raised for the room by any player.

        /// To distinguish the actors, each gets a consecutive actornumber. This is used in events to mark who triggered
        /// the event. A client finds out it's own actornumber in the return callback for operation Join. Number 1 is the
        /// lowest actornumber in each room and the client with that actornumber created the room.

        /// Each client could easily send custom data around. If the data should be available to newcomers, it makes sense
        /// to use Properties.

        /// Joining a room will trigger the event <see cref="F:ExitGames.Client.Photon.Lite.LiteEventCode.Join" text="LiteEventCode.Join"/>, which contains
        /// the list of actorNumbers of current players inside the  room
        /// (<see cref="F:ExitGames.Client.Photon.Lite.LiteEventKey.ActorList" text="LiteEventKey.ActorList"/>). This also gives you a count of current
        /// players.
        /// </summary>

        /// <param name="gameName">Any identifying name for a room / game.</param>
        /// <param name="gameProperties">optional, set of game properties, by convention: only used if game is new/created</param>
        /// <param name="actorProperties">optional, set of actor properties</param>
        /// <param name="broadcastActorProperties">optional, broadcast actor proprties in join-event</param>
        /// <returns>If operation could be enqueued for sending</returns>
        public virtual bool OpJoin(string gameName, Hashtable gameProperties, Hashtable actorProperties, bool broadcastActorProperties)
        {
            if (base.DebugOut >= DebugLevel.ALL)
            {
                base.Listener.DebugReturn(DebugLevel.ALL, "OpJoin(" + gameName + ")");
            }
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters[0xff] = gameName;
            if (actorProperties != null)
            {
                opParameters[0xf9] = actorProperties;
            }
            if (gameProperties != null)
            {
                opParameters[0xf8] = gameProperties;
            }
            if (broadcastActorProperties)
            {
                opParameters[250] = broadcastActorProperties;
            }
            return this.OpCustom(0xff, opParameters, true, 0, false);
        }

        ///<summary>
        /// Leave operation of the Lite Application (also in Lite Lobby).
        /// Leaves a room / game, but keeps the connection. This operations triggers the event <see cref="F:ExitGames.Client.Photon.Lite.LiteEventCode.Leave" text="LiteEventCode.Leave"/>
        /// for the remaining clients. The event includes the actorNumber of the player who left in key <see cref="F:ExitGames.Client.Photon.Lite.LiteEventKey.ActorNr" text="LiteEventKey.ActorNr"/>.
        ///</summary>
        ///<returns>
        /// Consecutive invocationID of the OP. Will throw Exception if not connected.
        ///</returns>
        public virtual bool OpLeave()
        {
            if (base.DebugOut >= DebugLevel.ALL)
            {
                base.Listener.DebugReturn(DebugLevel.ALL, "OpLeave()");
            }
            return this.OpCustom(0xfe, null, true, 0);
        }

        public virtual bool OpRaiseEvent(byte eventCode, bool sendReliable, object customEventContent)
        {
            return this.OpRaiseEvent(eventCode, sendReliable, customEventContent, 0, EventCaching.DoNotCache, null, ReceiverGroup.Others, 0);
        }

        ///<summary>
        /// RaiseEvent tells the server to send an event to the other players within the same room.
        ///</summary>
        ///<remarks>
        /// This method is described in one of its overloads.
        ///</remarks>
        ///<param name="eventCode">Identifies this type of event (and the content). Your game's event codes can start with 0.</param>
        ///<param name="customEventContent">Custom data you want to send along (use null, if none).</param>
        ///<param name="sendReliable">If this event has to arrive reliably (potentially repeated if it's lost).</param>
        ///<returns>If operation could be enqueued for sending</returns>
        public virtual bool OpRaiseEvent(byte eventCode, Hashtable customEventContent, bool sendReliable)
        {
            return this.OpRaiseEvent(eventCode, customEventContent, sendReliable, 0);
        }

        ///<summary>
        /// Send your custom data as event to an "interest group" in the current Room.
        ///</summary>
        ///<remarks>
        /// No matter if reliable or not, when an event is sent to a interest Group, some users won't get this data.
        /// Clients can control the groups they are interested in by using OpChangeGroups.
        ///</remarks>
        ///<param name="eventCode">Identifies this type of event (and the content). Your game's event codes can start with 0.</param>
        ///<param name="interestGroup">The ID of the interest group this event goes to (exclusively). Grouo 0 sends to all.</param>
        ///<param name="customEventContent">Custom data you want to send along (use null, if none).</param>
        ///<param name="sendReliable">If this event has to arrive reliably (potentially repeated if it's lost).</param>
        ///<returns>If operation could be enqueued for sending</returns>
        public virtual bool OpRaiseEvent(byte eventCode, byte interestGroup, Hashtable customEventContent, bool sendReliable)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters[0xf5] = customEventContent;
            opParameters[0xf4] = eventCode;
            if (interestGroup != 0)
            {
                opParameters[240] = interestGroup;
            }
            return this.OpCustom(0xfd, opParameters, sendReliable, 0);
        }

        ///<summary>
        /// RaiseEvent tells the server to send an event to the other players within the same room.
        /// </summary>
        /// <remarks>
        /// Type and content of the event can be defined by the client side at will. The server only
        /// forwards the content and eventCode to others in the same room.

        /// The eventCode should be used to define the event's type and content respectively.///
        /// Lite and Loadbalancing are using a few eventCode values already but those start with 255 and go down.
        /// Your eventCodes can start at 1, going up.

        /// The customEventContent is a Hashtable with any number of key-value pairs of
        /// <see cref="!:Serializable Datatypes" text="serializable datatypes"/> or null.
        /// Receiving clients can access this Hashtable as Parameter LiteEventKey.Data (see below).

        /// RaiseEvent can be used reliable or unreliable. Both result in ordered events but the unreliable ones
        /// might be lost and allow gaps in the resulting event sequence. On the other hand, they cause less
        /// overhead and are optimal for data that is replaced soon.

        /// Like all operations, RaiseEvent is not done immediately but when you call SendOutgoingCommands.

        /// It is recommended to keep keys (and data) as simple as possible (e.g. byte or short as key), as
        /// the data is typically sent multiple times per second. This easily adds up to a huge amount of data
        /// otherwise.
        /// </remarks>
        /// <example>
        /// <code>
        /// //send some position data (using byte-keys, as they are small):

        /// Hashtable evInfo = new Hashtable();
        /// Player local = (Player)players[playerLocalID];
        /// evInfo.Add((byte)STATUS_PLAYER_POS_X, (int)local.posX);
        /// evInfo.Add((byte)STATUS_PLAYER_POS_Y, (int)local.posY);

        /// peer.OpRaiseEvent(EV_MOVE, evInfo, true);  //EV_MOVE = (byte)1

        /// //receive this custom event in OnEvent():
        /// Hashtable data = (Hashtable)photonEvent[LiteEventKey.Data];
        /// switch (eventCode) {
        ///   case EV_MOVE:               //1 in this sample
        ///       p = (Player)players[actorNr];
        ///       if (p != null) {
        ///           p.posX = (int)data[(byte)STATUS_PLAYER_POS_X];
        ///           p.posY = (int)data[(byte)STATUS_PLAYER_POS_Y];
        ///       }
        ///       break;
        /// </code>

        /// Events from the Photon Server are internally buffered until they are
        /// <see cref="M:ExitGames.Client.Photon.PhotonPeer.DispatchIncomingCommands" text="Dispatched"/>, just
        /// like OperationResults.
        /// </example>
        /// <param name="eventCode">Identifies this type of event (and the content). Your game's event codes can start with 0.</param>
        /// <param name="customEventContent">Custom data you want to send along (use null, if none).</param>
        /// <param name="sendReliable">If this event has to arrive reliably (potentially repeated if it's lost).</param>
        /// <param name="channelId">Number of channel (sequence) to use (starting with 0).</param>
        /// <returns>If operation could be enqueued for sending.</returns>
        public virtual bool OpRaiseEvent(byte eventCode, Hashtable customEventContent, bool sendReliable, byte channelId)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters[0xf5] = customEventContent;
            opParameters[0xf4] = eventCode;
            return this.OpCustom(0xfd, opParameters, sendReliable, channelId);
        }

        ///<summary>
        /// RaiseEvent tells the server to send an event to the other players within the same room.
        /// </summary>
        /// <remarks>
        /// This method is described in one of its overloads.

        /// This variant has an optional list of targetActors. Use this to send the event only to
        /// specific actors in the same room, each identified by an actorNumber (or ID).

        /// This can be useful to implement private messages inside a room or similar.
        /// </remarks>
        /// <param name="eventCode">Identifies this type of event (and the content). Your game's event codes can start with 0.</param>
        /// <param name="customEventContent">Custom data you want to send along (use null, if none).</param>
        /// <param name="sendReliable">If this event has to arrive reliably (potentially repeated if it's lost).</param>
        /// <param name="channelId">Number of channel to use (starting with 0).</param>
        /// <param name="targetActors">List of actorNumbers that receive this event.</param>
        /// <returns>If operation could be enqueued for sending.</returns>
        public virtual bool OpRaiseEvent(byte eventCode, Hashtable customEventContent, bool sendReliable, byte channelId, int[] targetActors)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters[0xf5] = customEventContent;
            opParameters[0xf4] = eventCode;
            if (targetActors != null)
            {
                opParameters[0xfc] = targetActors;
            }
            return this.OpCustom(0xfd, opParameters, sendReliable, channelId);
        }

        ///<summary>
        /// Calls operation RaiseEvent on the server, with full control of event-caching and the target receivers.
        /// </summary>
        /// <remarks>
        /// This method is described in one of its overloads.

        /// The cache parameter defines if and how this event will be cached server-side. Per event-code, your client
        /// can store events and update them and will send cached events to players joining the same room.

        /// The option EventCaching.DoNotCache matches the default behaviour of RaiseEvent.
        /// The option EventCaching.MergeCache will merge the costomEventContent into existing one.
        /// Values in the customEventContent Hashtable can be null to remove existing values.

        /// With the receivers parameter, you can chose who gets this event: Others (default), All (includes you as sender)
        /// or MasterClient. The MasterClient is the connected player with the lowest ActorNumber in this room.
        /// This player could get some privileges, if needed.

        /// Read more about Cached Events in the DevNet: http://doc.exitgames.com
        /// </remarks>
        /// <param name="eventCode">Identifies this type of event (and the content). Your game's event codes can start with 0.</param>
        /// <param name="customEventContent">Custom data you want to send along (use null, if none).</param>
        /// <param name="sendReliable">If this event has to arrive reliably (potentially repeated if it's lost).</param>
        /// <param name="channelId">Number of channel to use (starting with 0).</param>
        /// <param name="cache">Events can be cached (merged and removed) for players joining later on.</param>
        /// <param name="receivers">Controls who should get this event.</param>
        /// <returns>If operation could be enqueued for sending.</returns>
        public virtual bool OpRaiseEvent(byte eventCode, Hashtable customEventContent, bool sendReliable, byte channelId, EventCaching cache, ReceiverGroup receivers)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters[0xf5] = customEventContent;
            opParameters[0xf4] = eventCode;
            if (cache != EventCaching.DoNotCache)
            {
                opParameters[0xf7] = (byte)cache;
            }
            if (receivers != ReceiverGroup.Others)
            {
                opParameters[0xf6] = (byte)receivers;
            }
            return this.OpCustom(0xfd, opParameters, sendReliable, channelId, false);
        }

        ///<summary>
        /// Calls operation RaiseEvent on the server, with full control of event-caching and the target receivers.
        /// </summary>
        /// <remarks>
        /// This method is described in one of its overloads.

        /// The cache parameter defines if and how this event will be cached server-side. Per event-code, your client
        /// can store events and update them and will send cached events to players joining the same room.

        /// The option EventCaching.DoNotCache matches the default behaviour of RaiseEvent.
        /// The option EventCaching.MergeCache will merge the costomEventContent into existing one.
        /// Values in the customEventContent Hashtable can be null to remove existing values.

        /// With the receivers parameter, you can chose who gets this event: Others (default), All (includes you as sender)
        /// or MasterClient. The MasterClient is the connected player with the lowest ActorNumber in this room.
        /// This player could get some privileges, if needed.

        /// Read more about Cached Events in the DevNet: http://doc.exitgames.com
        /// </remarks>
        /// <param name="eventCode">Identifies this type of event (and the content). Your game's event codes can start with 0.</param>
        /// <param name="customEventContent">Custom data you want to send along (use null, if none).</param>
        /// <param name="sendReliable">If this event has to arrive reliably (potentially repeated if it's lost).</param>
        /// <param name="channelId">Number of channel to use (starting with 0).</param>
        /// <param name="cache">Events can be cached (merged and removed) for players joining later on.</param>
        /// <param name="receivers">Controls who should get this event.</param>
        /// <returns>If operation could be enqueued for sending.</returns>
        public virtual bool OpRaiseEvent(byte eventCode, bool sendReliable, object customEventContent, byte channelId, EventCaching cache, int[] targetActors, ReceiverGroup receivers, byte interestGroup)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters[0xf4] = eventCode;
            if (customEventContent != null)
            {
                opParameters[0xf5] = customEventContent;
            }
            if (cache != EventCaching.DoNotCache)
            {
                opParameters[0xf7] = (byte)cache;
            }
            if (receivers != ReceiverGroup.Others)
            {
                opParameters[0xf6] = (byte)receivers;
            }
            if (interestGroup != 0)
            {
                opParameters[240] = interestGroup;
            }
            if (targetActors != null)
            {
                opParameters[0xfc] = targetActors;
            }
            return this.OpCustom(0xfd, opParameters, sendReliable, channelId, false);
        }

        ///<summary>
        /// Attaches or updates properties of the specified actor.
        ///</summary>
        ///<remarks>
        /// Please read the general description of <see cref="!:Properties on Photon"/>.
        ///</remarks>
        ///<param name="properties">Hashtable containing the properties to add or update.</param>
        ///<param name="actorNr">the actorNr is used to identify a player/peer in a game</param>
        ///<param name="broadcast">true will trigger an event LiteEventKey.PropertiesChanged with the updated properties in it</param>
        ///<param name="channelId">Number of channel to use (starting with 0).</param>
        ///<returns>If operation could be enqueued for sending</returns>
        public virtual bool OpSetPropertiesOfActor(int actorNr, Hashtable properties, bool broadcast, byte channelId)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(0xfb, properties);
            opParameters.Add(0xfe, actorNr);
            if (broadcast)
            {
                opParameters.Add(250, broadcast);
            }
            return this.OpCustom(0xfc, opParameters, true, channelId);
        }

        ///<summary>
        /// Attaches or updates properties of the current game.
        ///</summary>
        ///<remarks>
        /// Please read the general description of <see cref="!:Properties on Photon"/>.
        ///</remarks>
        ///<param name="properties">hashtable containing the properties to add or overwrite</param>
        ///<param name="broadcast">true will trigger an event LiteEventKey.PropertiesChanged with the updated
        ///                        properties in it</param>
        ///<param name="channelId">Number of channel to use (starting with 0).</param>
        ///<returns>If operation could be enqueued for sending</returns>
        public virtual bool OpSetPropertiesOfGame(Hashtable properties, bool broadcast, byte channelId)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(0xfb, properties);
            if (broadcast)
            {
                opParameters.Add(250, broadcast);
            }
            return this.OpCustom(0xfc, opParameters, true, channelId);
        }
    }
}
