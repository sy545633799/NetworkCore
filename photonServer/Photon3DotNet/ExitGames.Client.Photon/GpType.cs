namespace ExitGames.Client.Photon
{
    /// <summary>
    /// The gp type. 
    /// </summary>
    internal enum GpType : byte
    {
        /// <summary>
        /// Unkown type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///  Null value don't have types.
        /// </summary>
        Null = 0x2a,

        /// <summary>
        /// A dictionary
        /// </summary>
        /// <remarks>This type is new in version 1.6.</remarks>
        Dictionary = 0x44,

        /// <summary>
        ///  An array of string values.
        /// </summary>
        StringArray = 0x61,

        /// <summary>
        /// A byte value.
        /// </summary>
        Byte = 0x62,

        /// <summary>
        ///  A costum type
        /// </summary>
        Custom = 0x63,

        /// <summary>
        /// A 64-bit floating-point value.//0x64
        /// </summary>
        /// <remarks>This type is new in version 1.5.</remarks>
        Double = 100,

        EventData = 0x65,

        /// <summary>
        ///  A 32-bit floating-point value.
        /// </summary>
        /// <remarks>This type is new in version 1.5.</remarks>
        Float = 0x66,

        /// <summary>
        /// A Hashtable.
        /// </summary>
        Hashtable = 0x68,

        /// <summary>
        /// A 32-bit integer value.
        /// </summary>
        Integer = 0x69,

        /// <summary>
        ///  A 16-bit integer value.
        /// </summary>
        Short = 0x6b,

        /// <summary>       
        /// <summary>
        /// A 64-bit integer value.
        /// </summary>
        Long = 0x6c,

        /// <summary>
        /// An array of 32-bit integer values. //0x6e
        /// </summary>
        IntegerArray = 110,

        ///  A boolean Value.
        /// </summary>
        Boolean = 0x6f,

        OperationResponse = 0x70,

        OperationRequest = 0x71,

        /// <summary>
        ///  A string value.
        /// </summary>
        String = 0x73,

        /// <summary>
        /// A vector.
        /// </summary>
        Vector = 0x76,

        /// <summary>
        ///  An array of bytes.//0x78
        /// </summary>
        ByteArray = 120,

        /// <summary>
        /// An array of objects.
        /// </summary>
        /// <remarks>This type is new in version 1.5.</remarks>
        Array = 0x79,

        /// <summary>
        /// An array of objects.
        /// </summary>
        ObjectArray = 0x7a
    }
}
