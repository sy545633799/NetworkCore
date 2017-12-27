
namespace ExitGames.Client.Photon.Lite
{
    using System;

    /// <summary>
    /// Lite - Flags for "types of properties", being used as filter in OpGetProperties. 
    /// </summary>
    [Flags]
    public enum LitePropertyTypes : byte
    {
        /// <summary>
        /// (0x00) Flag type for no property type.
        /// </summary>
        None = 0,

        /// <summary>
        /// (0x01) Flag type for game-attached properties.
        /// </summary>
        Game = 1,

        /// <summary>
        /// (0x02) Flag type for actor related propeties.
        /// </summary>
        Actor = 2,

        /// <summary>
        /// (0x01) Flag type for game AND actor properties. Equal to 'Game'
        /// </summary>
        GameAndActor = 3
    }
}
