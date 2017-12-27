namespace ExitGames.Client.Photon.Lite
{
    ///<summary>
    /// Lite - Keys of event-parameters that are defined by the Lite application logic.
    /// To keep things lean (in terms of bandwidth), we use byte keys to identify values in events within Photon.
    /// In Lite, you can send custom events by defining a EventCode and some content. This custom content is a Hashtable,
    /// which can use any type for keys and values. The parameter for operation RaiseEvent and the resulting
    /// Events use key (byte)245 for the custom content. The constant for this is: Data or
    ///<see cref="F:ExitGames.Client.Photon.Lite.LiteEventKey.CustomContent" text="LiteEventKey.CustomContent Field"/>.
    ///</summary>
    ///<remarks>
    /// If your game is built as extension of Lite, don't re-use these codes for your custom events.
    ///</remarks>
    public static class LiteEventKey
    {
        /// <summary>
        /// (254) Playernumber of the player who triggered the event.
        /// </summary>
        public const byte ActorNr = 0xfe;

        /// <summary>
        /// (253) Playernumber of the player who is target of an event (e.g. changed properties).
        /// </summary>
        public const byte TargetActorNr = 0xfd;

        /// <summary>
        /// (252) List of playernumbers currently in the room.
        /// </summary>
        public const byte ActorList = 0xfc;

        /// <summary>
        /// (251) Set of properties (a Hashtable).
        /// </summary>
        public const byte Properties = 0xfb;

        /// <summary>
        /// (249) Key for actor (player) property set (Hashtable).
        /// </summary>
        public const byte ActorProperties = 0xf9;

        /// <summary>
        /// (248) Key for game (room) property set (Hashtable).
        /// </summary>
        public const byte GameProperties = 0xf8;

        /// <summary>
        /// (245) Custom Content of an event (a Hashtable in Lite).
        /// </summary>
        public const byte Data = 0xf5;

        ///<summary>
        /// (245) The Lite operation RaiseEvent will place the Hashtable with your custom event-content under this key.</summary>
        ///<remarks>Alternative for: Data!</remarks>
        public const byte CustomContent = 0xf5;

    }
}
