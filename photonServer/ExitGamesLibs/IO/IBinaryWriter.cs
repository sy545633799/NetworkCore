using System.IO;

namespace ExitGames.IO
{
    /// <summary>
    /// The interface for a binary writer. 
    /// </summary>
    public interface IBinaryWriter
    {
        /// <summary>
        /// Writes a boolean. 
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteBoolean(bool value);

        /// <summary>
        /// Writes a byte. 
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteByte(byte value);

        /// <summary>
        /// Writes bytes. 
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteBytes(byte[] value);

        /// <summary>
        /// Writes a char. 
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteChar(char value);

        /// <summary>
        /// write double. 
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteDouble(double value);

        /// <summary>
        /// Writes a short. 
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteInt16(short value);

        /// <summary>
        /// Writes an integer. 
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteInt32(int value);

        /// <summary>
        /// Writes a long. 
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteInt64(long value);

        /// <summary>
        /// Writes a 32-bit floating point value. 
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteSingle(float value);

        /// <summary>
        /// Writes an utf8 encoded string. 
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteUTF(string value);

        /// <summary>
        /// Gets the underlying stream. 
        /// </summary>
        Stream Stream { get; }
    }
}
