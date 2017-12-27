using System;
using System.Security.Cryptography;
using Photon.SocketServer.Numeric;

namespace Photon.SocketServer.Security
{
    public class DiffieHellmanCryptoProvider : IDisposable
    {
        // Fields
        private Rijndael crypto;
        private readonly BigInteger prime = new BigInteger(OakleyGroups.OakleyPrime768);
        private static readonly BigInteger primeRoot = new BigInteger(OakleyGroups.Generator);
        private readonly BigInteger publicKey;
        private readonly BigInteger secret;
        private byte[] sharedKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Security.DiffieHellmanCryptoProvider"/> class.
        /// </summary>
        public DiffieHellmanCryptoProvider()
        {
            this.secret = this.GenerateRandomSecret(160);
            this.publicKey = this.CalculatePublicKey();
        }

        private BigInteger CalculatePublicKey()
        {
            return primeRoot.ModPow(this.secret, this.prime);
        }

        private BigInteger CalculateSharedKey(BigInteger otherPartyPublicKey)
        {
            return otherPartyPublicKey.ModPow(this.secret, this.prime);
        }

        public byte[] Decrypt(byte[] data)
        {
            return this.Decrypt(data, 0, data.Length);
        }

        public byte[] Decrypt(byte[] data, int offset, int count)
        {
            using (ICryptoTransform enc = this.crypto.CreateDecryptor())
            {
                return enc.TransformFinalBlock(data, offset, count);
            }
        }

        ///<summary>
        /// Derives the shared key is generated from the secret agreement between two parties, 
        /// given a byte array that contains the second party's public key. 
        ///</summary>
        ///<param name="otherPartyPublicKey">
        /// The second party's public key.
        ///</param>
        public void DeriveSharedKey(byte[] otherPartyPublicKey)
        {
            byte[] hash;
            BigInteger key = new BigInteger(otherPartyPublicKey);
            this.sharedKey = this.CalculateSharedKey(key).GetBytes();
            using (SHA256 hashProvider = new SHA256Managed())
            {
                hash = hashProvider.ComputeHash(this.SharedKey);
            }
            this.crypto = new RijndaelManaged();
            this.crypto.Key = hash;
            this.crypto.IV = new byte[0x10];
            this.crypto.Padding = PaddingMode.PKCS7;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
            }
        }

        public byte[] Encrypt(byte[] data)
        {
            return this.Encrypt(data, 0, data.Length);
        }

        public byte[] Encrypt(byte[] data, int offset, int count)
        {
            using (ICryptoTransform enc = this.crypto.CreateEncryptor())
            {
                return enc.TransformFinalBlock(data, offset, count);
            }
        }

        private BigInteger GenerateRandomSecret(int secretLength)
        {
            BigInteger result;
            do
            {
                result = BigInteger.GenerateRandom(secretLength);
            }
            while ((result >= (this.prime - 1)) || (result == 0));
            return result;
        }

        // Properties
        public bool IsInitialized
        {
            get
            {
                return (this.crypto != null);
            }
        }

        /// <summary>
        /// Gets the public key that can be used by another DiffieHellmanCryptoProvider object 
        /// to generate a shared secret agreement.
        /// </summary>
        public byte[] PublicKey
        {
            get
            {
                return this.publicKey.GetBytes();
            }
        }

        /// <summary>
        /// Gets the shared key that is used by the current instance for cryptographic operations.
        /// </summary>
        public byte[] SharedKey
        {
            get
            {
                return this.sharedKey;
            }
        }
    }
}
