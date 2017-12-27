namespace ExitGames.Client.Photon
{
    ///<summary>
    /// Type of deserialization methods to add custom type support.
    /// Use PhotonPeer.RegisterType() to register new types with serialization and deserialization methods.
    ///</summary>
    ///<param name="serializedCustomObject">The framwork passes in the data it got by the associated SerializeMethod. The type code and length are stripped and applied before a DeserializeMethod is called.</param>
    ///<returns>Return a object of the type that was associated with this method through RegisterType().</returns>
    public delegate object DeserializeMethod(byte[] serializedCustomObject);
}
