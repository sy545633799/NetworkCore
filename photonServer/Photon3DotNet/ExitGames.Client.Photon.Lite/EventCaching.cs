namespace ExitGames.Client.Photon.Lite
{
    ///<summary>
    /// Lite - OpRaiseEvent allows you to cache events and automatically send them to joining players in a room.
    /// Events are cached per event code and player: Event 100 (example!) can be stored once per player.
    /// Cached events can be modified, replaced and removed.
    ///</summary>
    ///<remarks>
    /// Caching works only combination with ReceiverGroup options Others and All.
    ///</remarks>
    public enum EventCaching : byte
    {
        /// <summary>
        /// Default value (not sent).
        /// </summary>
        DoNotCache = 0,

        /// <summary>
        /// Will merge this event's keys with those already cached.
        /// </summary>
        MergeCache = 1,

        /// <summary>
        /// Replaces the event cache for this eventCode with this event's content.
        /// </summary>
        ReplaceCache = 2,

        /// <summary>
        /// Removes this event (by eventCode) from the cache.
        /// </summary>
        RemoveCache = 3,

        /// <summary>
        /// Adds an event to the room's cache.
        /// </summary>
        AddToRoomCache = 4,

        /// <summary>
        /// Adds this event to the cache for actor 0 (becoming a "globally owned" event in the cache).
        /// </summary>
        AddToRoomCacheGlobal = 5,

        /// <summary>
        /// Remove fitting event from the room's cache.
        /// </summary>
        RemoveFromRoomCache = 6,

        /// <summary>
        /// Removes events of players who already left the room (cleaning up).
        /// </summary>
        RemoveFromRoomCacheForActorsLeft = 7,
    }
}
