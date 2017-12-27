using Photon.SocketServer.Numeric;

namespace Photon.SocketServer.Security
{
    /// <summary>
    ///  An implementation of the Diffie–Hellman key exchange.
    ///  Diffie–Hellman establishes a shared secret that can be 
    ///  used for secret communications by exchanging data over a public network.
    /// </summary>
    public class DiffieHellmanKeyExchange
    {
        /// <summary>
        /// The prime root.
        /// </summary>
        private static readonly BigInteger defaultPrimeRoot = new BigInteger(OakleyGroups.Generator);

        /// <summary>
        ///  The prime.
        /// </summary>
        private readonly BigInteger prime = new BigInteger(OakleyGroups.OakleyPrime768);

        /// <summary>
        /// The public key.
        /// </summary>
        private readonly BigInteger publicKey;

        /// <summary>
        /// The secret.
        /// </summary>
        private readonly BigInteger secret;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Security.DiffieHellmanKeyExchange"/> class.
        /// </summary>
        public DiffieHellmanKeyExchange()
        {
            secret = GenerateRandomSecret(160);
            publicKey = CalculatePublicKey();
            PublicKey = publicKey.GetBytes();
        }

        /// <summary>
        /// Calculates the public key.
        /// </summary>
        /// <returns>A <see cref="T:Photon.SocketServer.Numeric.BigInteger"/>.</returns>
        private BigInteger CalculatePublicKey()
        {
            return defaultPrimeRoot.ModPow(secret, prime);
        }

        /// <summary>
        /// Calculates the shared key.
        /// </summary>
        /// <param name="otherPartyPublicKey">The other party public key.</param>
        /// <returns>A <see cref="T:Photon.SocketServer.Numeric.BigInteger"/>.</returns>
        private BigInteger CalculateSharedKey(BigInteger otherPartyPublicKey)
        {
            return otherPartyPublicKey.ModPow(secret, prime);
        }

        /// <summary>
        /// Derives the shared key.
        /// </summary>
        /// <param name="otherPartyPublicKey">The others party public key.</param>
        public void DeriveSharedKey(byte[] otherPartyPublicKey)
        {
            BigInteger integer = new BigInteger(otherPartyPublicKey);
            SharedKey = CalculateSharedKey(integer).GetBytes();
        }

        /// <summary>
        /// Generates a random secret.
        /// </summary>
        /// <param name="secretLength">The secret length.</param>
        /// <returns>A <see cref="T:Photon.SocketServer.Numeric.BigInteger"/>.</returns>
        /// <remarks>
        /// Parameter requirements:
        /// The private key x be in the interval [2, (q - 2)].
        /// http://tools.ietf.org/html/rfc2631#section-2.2
        ///</remarks>
        private BigInteger GenerateRandomSecret(int secretLength)
        {
            BigInteger integer;
            do
            {
                integer = BigInteger.GenerateRandom(secretLength);
            }
            while (integer >= (prime - 1) || integer < 2);
            return integer;
        }

        // Properties
        internal BigInteger Prime
        {
            get
            {
                return prime;
            }
        }

        /// <summary>
        /// Gets the public key which can be used by the other party to derive the 
        ///  shared key.
        /// </summary>
        public byte[] PublicKey { get; private set; }

        internal BigInteger Secret
        {
            get
            {
                return secret;
            }
        }

        /// <summary>
        /// Gets the shared key that which can be used as the key for cryptographic operations.
        /// </summary>
        public byte[] SharedKey { get; private set; }
    }

}
