namespace ExitGames.Client.Photon
{
    using System.Collections.Generic;

    ///<summary>
    /// Contains all components of a Photon Event. 
    /// Event Parameters, like OperationRequests and OperationResults, consist of a Dictionary with byte-typed keys per value.
    ///</summary>
    ///<remarks>
    /// The indexer of this class actually provides access to the Parameters Dictionary.
    /// The operation RaiseEvent of the Lite application allows you to provide custom event content. Defined in Lite, this
    /// CustomContent will be made the value of key LiteEventKey.OperationRaiseEvent which is (byte)42.
    /// Enums and constants for the Lite-Application codes are defined in the LitePeer namespace. Check: LiteEventKey, etc.
    ///</remarks>
    public class EventData
    {
        /// <summary>
        /// The event code identifies the type of event.
        /// </summary>
        public byte Code;

        public Dictionary<byte, object> Parameters;

        /// <summary>
        /// ToString() override.
        /// </summary>
        /// <returns>Short output of "Event" and it's Code.</returns>
        public override string ToString()
        {
            return string.Format("Event {0}.", this.Code.ToString());
        }

        /// <summary>
        /// Extensive output of the event content.
        /// </summary>
        /// <returns>To be used in debug situations only, as it returns a string for each value.</returns>
        public string ToStringFull()
        {
            return string.Format("Event {0}: {1}", this.Code, SupportClass.DictionaryToString(this.Parameters));
        }

        /// <summary>
        /// Alternative access to the Parameters.
        /// </summary>
        /// <param name="key">The key byte-code of a event value.</param>
        /// <returns>The Parameters value, or null if the key does not exist in Parameters.</returns>
        public object this[byte key]
        {
            get
            {
                object o;
                this.Parameters.TryGetValue(key, out o);
                return o;
            }
            set
            {
                this.Parameters[key] = value;
            }
        }
    }
}
