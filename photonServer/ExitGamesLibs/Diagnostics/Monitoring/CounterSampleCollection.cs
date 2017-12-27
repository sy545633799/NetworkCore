using System;
using System.Collections.ObjectModel;
using System.IO;
using ExitGames.Diagnostics.Counter;

namespace ExitGames.Diagnostics.Monitoring
{
    /// <summary>
    /// A collectin of <see cref="T:ExitGames.Diagnostics.Counter.CounterSample"/>s.
    /// </summary>
    public class CounterSampleCollection : Collection<CounterSample>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleCollection"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public CounterSampleCollection(string name)
        {
            CounterName = name;
        }

        /// <summary>
        /// Deserializes a stream of <see cref="T:ExitGames.Diagnostics.Counter.CounterSample"/>s.
        /// </summary>
        /// <param name="binaryReader">The reader wrapping a stream.</param>
        /// <returns>A new <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleCollection"/>.</returns>
        public static CounterSampleCollection Deserialize(BinaryReader binaryReader)
        {
            string name = binaryReader.ReadString();
            short count = binaryReader.ReadInt16();
            CounterSampleCollection samples = new CounterSampleCollection(name);
            for (int i = 0; i < count; i++)
            {
                long dateData = binaryReader.ReadInt64();
                DateTime timestamp = DateTime.FromBinary(dateData);
                float value = binaryReader.ReadSingle();
                samples.Add(new CounterSample(timestamp, value));
            }
            return samples;
        }

        /// <summary>
        ///  Deserializes a stream of <see cref="T:ExitGames.Diagnostics.Counter.CounterSample"/>s.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>A new <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleCollection"/>.</returns>
        public static CounterSampleCollection Deserialize(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);
            return Deserialize(binaryReader);
        }

        /// <summary>
        /// Serializes the <see cref="T:ExitGames.Diagnostics.Counter.CounterSample"/>s to a stream with a <see cref="T:System.IO.BinaryWriter"/>.
        /// </summary>
        /// <param name="binaryWriter">The binary writer wrapping a stream.</param>
        public void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(this.CounterName);
            binaryWriter.Write((short)base.Count);
            foreach (CounterSample sample in this)
            {
                binaryWriter.Write(sample.Timestamp.ToBinary());
                binaryWriter.Write(sample.Value);
            }
        }

        /// <summary>
        /// Serializes the <see cref="T:ExitGames.Diagnostics.Counter.CounterSample"/>s to a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Serialize(Stream stream)
        {
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            this.Serialize(binaryWriter);
        }

        /// <summary>
        /// Gets the counter name.
        /// </summary>
        public string CounterName { get; private set; }
    }
}
