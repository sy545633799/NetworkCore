namespace Photon.SocketServer.Security
{
    /// <summary>
    /// Provides methods to encrypt and decrypt binary data. 
    /// </summary>
    public interface ICryptoProvider
    {
        /// <summary>
        /// Decrypts the specified data. 
        /// </summary>
        /// <param name="data">The data to decrypt.</param>
        /// <returns>A byte array containing the decrypted data.</returns>
        byte[] Decrypt(byte[] data);

        /// <summary>
        /// Decrypts the specified data. 
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>A byte array containing the decrypted data.</returns>
        byte[] Decrypt(byte[] data, int offset, int count);

        /// <summary>
        /// Encrypts the specified data. 
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <returns>A byte array containing the encrypted data.</returns>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// Encrypts the specified data. 
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>A byte array containing the encrypted data.</returns>
        byte[] Encrypt(byte[] data, int offset, int count);

        /// <summary>
        /// Gets a value indicating whether IsInitialized. 
        /// </summary>
        bool IsInitialized { get; }
    }
}
