using System;

namespace ExitGames.Diagnostics.Monitoring
{
    /// <summary>
    /// Attribute for <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/> fields or properties to publish with the <see 
    /// cref="T:ExitGames.Diagnostics.Monitoring.CounterSamplePublisher"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class PublishCounterAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Monitoring.PublishCounterAttribute"/> class.
        /// </summary>
        public PublishCounterAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Monitoring.PublishCounterAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public PublishCounterAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
    }
}
