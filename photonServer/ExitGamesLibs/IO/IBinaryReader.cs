using System.IO;

namespace ExitGames.IO
{
    /// <summary>
    /// Interface for a binary reader. 
    /// </summary>
    public interface IBinaryReader
    {
        /// <summary>
        /// Reads bytes from the stream. 
        /// </summary>
        /// <param name="buffer">The buffer. </param>
        /// <param name="offset">The offset. </param>
        /// <param name="count">The count.</param>
        /// <returns>The read. </returns>
        int Read(byte[] buffer, int offset, int count);

        /// <summary>
        /// Reads a boolean from the stream. 
        /// </summary>
        /// <returns>The read boolean. </returns>
        bool ReadBoolean();

        /// <summary>
        /// Reads a byte from the stream. 
        /// </summary>
        /// <returns>The read byte. </returns>
        byte ReadByte();

        /// <summary>
        /// Reads bytes from the stream. 
        /// </summary>
        /// <param name="length">The length. </param>
        /// <returns>A byte array. </returns>
        byte[] ReadBytes(int length);

        /// <summary>
        /// Reads a char from the stream. 
        /// </summary>
        /// <returns>The read char. </returns>
        char ReadChar();

        /// <summary>
        /// Reads a double from the stream. 
        /// </summary>
        /// <returns>The read double. </returns>
        double ReadDouble();

        /// <summary>
        /// Reads a short from the stream. 
        /// </summary>
        /// <returns>The read short. </returns>
        short ReadInt16();

        /// <summary>
        /// Reads an integer from the stream. 
        /// </summary>
        /// <returns>The read integer. </returns>
        int ReadInt32();

        /// <summary>
        /// Eeads a long from the stream. 
        /// </summary>
        /// <returns></returns>
        long ReadInt64();

        /// <summary>
        /// The read long. 
        /// </summary>
        /// <returns>The read float. </returns>
        float ReadSingle();

        /// <summary>
        /// Gets the base stream. 
        /// </summary>
        Stream BaseStream { get; }
    }
}
