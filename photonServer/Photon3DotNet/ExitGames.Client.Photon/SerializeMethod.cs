namespace ExitGames.Client.Photon
{
    /// <summary>
    /// Type of serialization methods to add custom type support. Use PhotonPeer.ReisterType() to register new types with serialization and deserialization methods.
    /// </summary>
    /// <param name="customObject">The method will get objects passed that were registered with it in RegisterType().</param>
    /// <returns>Return a byte[] that resembles the object passed in. The framework will surround it with length and type info, so don't include it.</returns>
    public delegate byte[] SerializeMethod(object customObject);
}
