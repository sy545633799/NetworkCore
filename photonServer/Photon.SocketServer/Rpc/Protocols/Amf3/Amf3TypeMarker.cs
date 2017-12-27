namespace Photon.SocketServer.Rpc.Protocols.Amf3
{
    /// <summary>
    /// A type marker is one byte in length and describes the type of encoded data that follows. There are 13 types in AMF 3. 
    /// </summary>
    internal enum Amf3TypeMarker : byte
    {
        /// <summary>
        /// Undefined value. 
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Null value.
        /// </summary>
        Null = 1,

        /// <summary>
        /// False value.
        /// </summary>
        False = 2,

        /// <summary>
        /// True value. 
        /// </summary>
        True = 3,

        /// <summary>
        /// Integer value. 
        /// </summary>
        Integer = 4,

        /// <summary>
        /// Double value.
        /// </summary>
        Double = 5,

        /// <summary>
        /// String value. 
        /// </summary>
        String = 6,

        /// <summary>
        /// Xml doc value. 
        /// </summary>
        XmlDoc = 7,

        /// <summary>
        /// Date value. 
        /// </summary>
        Date = 8,

        /// <summary>
        /// Array value. 
        /// </summary>
        Array = 9,

        /// <summary>
        /// Object value. 
        /// </summary>
        Object = 10,

        /// <summary>
        /// Xml value. 
        /// </summary>
        Xml = 11,

        /// <summary>
        /// Byte array. 
        /// </summary>
        ByteArray = 12
    }
}
