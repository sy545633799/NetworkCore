using System;

namespace ExitGames.Diagnostics.Monitoring
{
    /// <summary>
    /// Attribute to set a name for a group of counters. 
    /// </summary>
    public class CounterSetAttribute :Attribute
    {
        /// <summary>
        /// Gets or sets the name. 
        /// </summary>
        public string Name { get; set; }
    }
}
