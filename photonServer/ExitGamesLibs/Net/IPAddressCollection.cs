using System.Collections.Generic;
using System.Net;

namespace ExitGames.Net
{
    /// <summary>
    /// The IP address collection. 
    /// </summary>
    public sealed class IPAddressCollection : List<IPAddress>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.IPAddressCollection"/> class.
        /// </summary>
        public IPAddressCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.IPAddressCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public IPAddressCollection(IEnumerable<IPAddress> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.IPAddressCollection"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public IPAddressCollection(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Add a new IP address.
        /// </summary>
        /// <param name="addressToAdd">The IP address to add.</param>
        public void Add(string addressToAdd)
        {
            IPAddress item = IPAddress.Parse(addressToAdd);
            base.Add(item);
        }

        /// <summary>
        /// Checks whether the collection contains an IP address.
        /// </summary>
        /// <param name="addressToCheck">The addressToCheck.</param>
        /// <returns>The contains.</returns>
        public bool Contains(string addressToCheck)
        {
            IPAddress item = IPAddress.Parse(addressToCheck);
            return base.Contains(item);
        }

        /// <summary>
        /// Removes an ip address.
        /// </summary>
        /// <param name="addressToRemove">The address.</param>
        /// <returns>The remove.</returns>
        public bool Remove(string addressToRemove)
        {
            IPAddress item = IPAddress.Parse(addressToRemove);
            return base.Remove(item);
        }
    }
}
