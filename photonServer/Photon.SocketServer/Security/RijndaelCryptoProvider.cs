using System;
using System.Security.Cryptography;
using ExitGames.Logging;

namespace Photon.SocketServer.Security
{
    /// <summary>
    /// An <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/> implementation using the Rijndael symmetric encryption algorithm.
    /// </summary>
    public class RijndaelCryptoProvider : ICryptoProvider
    {
        /// <summary>
        /// The symmetric algorithm.
        /// </summary>
        private readonly Rijndael crypto;

        /// <summary>
        /// An <see cref="T:ExitGames.Logging.ILogger"/> instance used to log to the logging framework.
        /// </summary>
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Security.RijndaelCryptoProvider"/> class.
        /// </summary>
        /// <param name="secretKey">The secret key for the symmetric algorithm.
        /// This algorithm supports key lengths of 128, 192, or 256 bits (16, 24 or 32 bytes).
        /// </param>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">
        /// The key size is invalid.
        /// This algorithm supports key lengths of 128, 192, or 256 bits (16, 24 or 32 bytes).
        /// </exception>
        /// <remarks>
        /// The secret key is used both for encryption and for decryption. For a symmetric algorithm to be successful, 
        ///  the secret key must be known only to the sender and the receiver.
        /// </remarks>
        public RijndaelCryptoProvider(byte[] secretKey)
            : this(secretKey, PaddingMode.PKCS7)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Security.RijndaelCryptoProvider"/> class.
        /// </summary>
        /// <param name="secretKey">The secret key for the symmetric algorithm.
        /// This algorithm supports key lengths of 128, 192, or 256 bits.</param>
        /// <param name="paddingMode">The padding mode.</param>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">
        ///The key size is invalid.
        ///This algorithm supports key lengths of 128, 192, or 256 bits (16, 24 or 32 bytes).
        ///</exception>
        ///<remarks>
        /// The secret key is used both for encryption and for decryption. For a symmetric algorithm to be successful, 
        ///the secret key must be known only to the sender and the receiver.
        ///</remarks>
        public RijndaelCryptoProvider(byte[] secretKey, PaddingMode paddingMode)
        {
            crypto = Rijndael.Create();
            crypto.Key = secretKey;
            crypto.IV = new byte[0x10];
            crypto.Padding = paddingMode;
        }

        /// <summary>
        /// Decrypts the specified data.
        /// </summary>
        /// <param name="data">The data to decrypt.</param>
        /// <returns>A byte array containing the decrypted data.</returns>
        public byte[] Decrypt(byte[] data)
        {
            return Decrypt(data, 0, data.Length);
        }

        /// <summary>
        /// Decrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>A byte array containing the decrypted data.</returns>
        public byte[] Decrypt(byte[] data, int offset, int count)
        {
            try
            {
                if (count < 0)
                {
                    if (logger.IsDebugEnabled)
                    {
                        logger.DebugFormat("Failed to decrypt data: offset={0}, count={1}, padding={2}, key={3}, data={4}", offset, count, crypto.Padding, BitConverter.ToString(crypto.Key), BitConverter.ToString(data));
                    }
                    return null;
                }
                using (ICryptoTransform transform = crypto.CreateDecryptor())
                {
                    return transform.TransformFinalBlock(data, offset, count);
                }
            }
            catch (CryptographicException exception)
            {
                if (logger.IsDebugEnabled)
                {
                    logger.DebugFormat("Failed to decrypt data: msg={0}, offset={1}, count={2}, padding={3}, key={4}, data={5}", exception.Message, offset, count, crypto.Padding, BitConverter.ToString(crypto.Key), BitConverter.ToString(data));
                }
            }
            return null;
        }

        /// <summary>
        /// Encrypts the specified data.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <returns>A byte array containing the encrypted data.</returns>
        public byte[] Encrypt(byte[] data)
        {
            return Encrypt(data, 0, data.Length);
        }

        /// <summary>
        /// Encrypts the specified data.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>A byte array containing the encrypted data.</returns>
        public byte[] Encrypt(byte[] data, int offset, int count)
        {
            using (ICryptoTransform transform = crypto.CreateEncryptor())
            {
                return transform.TransformFinalBlock(data, offset, count);
            }
        }

        /// <summary>
        /// Gets a value indicating whether IsInitialized.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return (crypto != null);
            }
        }

        /// <summary>
        /// Gets the padding mode used in the symmetric algorithm. The default is PaddingMode.PKCS7
        /// </summary>
        /// <value>The padding.</value>
        public PaddingMode Padding
        {
            get
            {
                return crypto.Padding;
            }
        }
    }
}
