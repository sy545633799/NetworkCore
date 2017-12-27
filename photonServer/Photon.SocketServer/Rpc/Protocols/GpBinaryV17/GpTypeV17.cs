namespace Photon.SocketServer.Rpc.Protocols.GpBinaryV17
{
    /// <summary>
    /// The gp type. 
    /// </summary>
    internal enum GpTypeV17 : byte
    {
        /// <summary>
        /// Unkown type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// An array of objects.
        /// </summary>
        Array = 0x79,

        /// <summary>
        /// A boolean Value.
        /// </summary>
        Boolean = 0x6f,

        /// <summary>
        /// A byte value.
        /// </summary>
        Byte = 0x62,

        /// <summary>
        /// An array of bytes.
        /// </summary>
        ByteArray = 120,

        /// <summary>
        /// An array of objects.
        /// </summary>
        ObjectArray = 0x7a,

        /// <summary>
        /// A 16-bit integer value.
        /// </summary>
        Short = 0x6b,

        /// <summary>
        /// A 32-bit floating-point value.
        /// </summary>
        /// <remarks>This type is new in version 1.5.</remarks>
        Float = 0x66,

        /// <summary>
        ///  A dictionary
        /// </summary>
        /// <remarks>This type is new in version 1.6.</remarks>
        Dictionary = 0x44,

        /// <summary>
        ///  A 64-bit floating-point value.
        /// </summary>
        /// <remarks>This type is new in version 1.5.</remarks>
        Double = 100,

        /// <summary>
        /// A Hashtable.
        /// </summary>
        Hashtable = 0x68,

        /// <summary>
        /// A 32-bit integer value.
        /// </summary>
        Integer = 0x69,

        /// <summary>
        /// An array of 32-bit integer values.
        /// </summary>
        IntegerArray = 110,

        /// <summary>
        ///  A 64-bit integer value.
        /// </summary>
        Long = 0x6c,

        /// <summary>
        /// A string value.
        /// </summary>
        String = 0x73,

        /// <summary>
        ///  A vector.
        /// </summary>
        Vector = 0x76,

        /// <summary>
        /// A costum type
        /// </summary>
        Custom = 0x63,

        /// <summary>
        /// Null value don't have types.
        /// </summary>
        Null = 0x2a,

        BooleanArray = 0xef,
        CompressedInt = 3,
        CompressedIntArray = 0x83,
        CompressedLong = 4,
        CompressedLongArray = 0x84,
        CustomTypeArray = 0xe3,
        DictionaryArray = 0xc4,
        DoubleArray = 0xe4,
        EventData = 0x65,
        FloatArray = 230,
        HashtableArray = 0xe8,
        Int1 = 1,
        Int2 = 2,
        OperationRequest = 0x71,
        OperationResponse = 0x70,
        ShortArray = 0xeb,
        StringArray = 0xf3,
    }
}
