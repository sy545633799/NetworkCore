namespace Photon.SocketServer
{
    /// <summary>
    /// Specifies the encryption method for <see cref="M:Photon.SocketServer.PeerBase.InitializeEncryption(System.Byte[],Photon.SocketServer.EncryptionMethod)"/>.
    /// </summary>
    public enum EncryptionMethod : byte
    {
        /// <summary>
        /// An SHA256 hash is used as the secret with a PKCS7 padding.
        /// </summary>
        Sha256Pkcs7 = 0,
        /// <summary>
        /// An MD5 hash is used as the secret with a ISO10126 padding
        /// </summary>
        Md5Iso10126 = 1

    }
}
