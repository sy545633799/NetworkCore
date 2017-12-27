using System;

namespace ExitGames.Configuration
{
    /// <summary>
    /// A configuration exception. 
    /// </summary>
    public class ConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Configuration.ConfigurationException"/> class.
        /// </summary>
        /// <param name="msg">The message.</param>
        public ConfigurationException(string msg)
            : base(msg)
        {
        }
    }
}
