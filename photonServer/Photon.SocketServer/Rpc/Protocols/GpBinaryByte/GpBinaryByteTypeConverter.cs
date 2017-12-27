using System;
using System.Collections;
using System.Collections.Generic;
using Photon.SocketServer.Rpc.ValueTypes;

namespace Photon.SocketServer.Rpc.Protocols.GpBinaryByte
{
    /// <summary>
    /// The gp binary byte type converter.
    /// </summary>
    internal static class GpBinaryByteTypeConverter
    {
        /// <summary>
        ///  The get clr array type.
        /// </summary>
        /// <param name="gpType">The gp type.</param>
        /// <returns>the gpType</returns>
        public static Type GetClrArrayType(GpType gpType)
        {
            switch (gpType)
            {
                case GpType.Byte:
                    return typeof(byte);

                case GpType.Double:
                    return typeof(double);

                case GpType.EventData:
                    return typeof(EventData);

                case GpType.Float:
                    return typeof(float);

                case GpType.Hashtable:
                    return typeof(Hashtable);

                case GpType.Integer:
                    return typeof(int);

                case GpType.Short:
                    return typeof(short);

                case GpType.Long:
                    return typeof(long);

                case GpType.Boolean:
                    return typeof(bool);

                case GpType.OperationResponse:
                    return typeof(OperationResponse);

                case GpType.OperationRequest:
                    return typeof(OperationRequest);

                case GpType.String:
                    return typeof(string);

                case GpType.Vector:
                    return typeof(ArrayList);

                case GpType.ByteArray:
                    return typeof(byte[]);
            }
            return null;
        }

        /// <summary>
        ///  Gets the type for an array.
        /// </summary>
        /// <param name="gpType"> The gp type.</param>
        /// <param name="size"> The size per item.</param>
        /// <returns> The type for the array.</returns>
        public static Type GetClrArrayType(GpType gpType, ref int size)
        {
            switch (gpType)
            {
                case GpType.Byte:
                    size = 1;
                    return typeof(byte);

                case GpType.Double:
                    size = 8;
                    return typeof(double);

                case GpType.EventData:
                    return typeof(EventData);

                case GpType.Float:
                    size = 4;
                    return typeof(float);

                case GpType.Hashtable:
                    size = 2;
                    return typeof(Hashtable);

                case GpType.Integer:
                    size = 4;
                    return typeof(int);

                case GpType.Short:
                    size = 2;
                    return typeof(short);

                case GpType.Long:
                    size = 8;
                    return typeof(long);

                case GpType.Boolean:
                    size = 1;
                    return typeof(bool);

                case GpType.OperationResponse:
                    return typeof(OperationResponse);

                case GpType.OperationRequest:
                    return typeof(OperationRequest);

                case GpType.String:
                    size = 1;
                    return typeof(string);

                case GpType.Vector:
                    return typeof(ArrayList);

                case GpType.ByteArray:
                    return typeof(byte[]);

                case GpType.Array:
                    size = 3;
                    return typeof(Array);
            }
            return null;
        }

        /// <summary>
        /// The get gp type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>the gpType</returns>
        public static GpType GetGpType(Type type)
        {
            if (type == null)
            {
                return GpType.Null;
            }
            GpType gpType = GetGpType(Type.GetTypeCode(type));
            if (gpType != GpType.Unknown)
            {
                return gpType;
            }
            if (type == typeof(Hashtable))
            {
                return GpType.Hashtable;
            }
            if (typeof(IEventData).IsAssignableFrom(type))
            {
                return GpType.EventData;
            }
            if (type == typeof(OperationResponse))
            {
                return GpType.OperationResponse;
            }
            if (type == typeof(OperationRequest))
            {
                return GpType.OperationRequest;
            }
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                if (elementType == typeof(byte))
                {
                    return GpType.ByteArray;
                }
                if (elementType == typeof(object))
                {
                    return GpType.ObjectArray;
                }
                return GpType.Array;
            }
            if (typeof(IList).IsAssignableFrom(type))
            {
                return GpType.Vector;
            }
            if (type == typeof(RawCustomValue))
            {
                return GpType.Custom;
            }
            if (type.IsGenericType)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                if (typeof(Dictionary<,>) == genericTypeDefinition)
                {
                    return GpType.Dictionary;
                }
            }
            return GpType.Unknown;
        }
       
        /// <summary>
        /// The get gp type.
        /// </summary>
        /// <param name="typeCode"> The type code.</param>
        /// <returns>the gpType</returns>
        public static GpType GetGpType(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return GpType.Boolean;

                case TypeCode.SByte:
                case TypeCode.Byte:
                    return GpType.Byte;

                case TypeCode.Int16:
                    return GpType.Short;

                case TypeCode.Int32:
                    return GpType.Integer;

                case TypeCode.Int64:
                    return GpType.Long;

                case TypeCode.Single:
                    return GpType.Float;

                case TypeCode.Double:
                    return GpType.Double;

                case TypeCode.String:
                    return GpType.String;
            }
            return GpType.Unknown;
        }

        /// <summary>
        /// Gets the minimum size for a type.
        /// </summary>
        /// <param name="gpType"> The gp type.</param>
        /// <returns> The minimum size.</returns>
        public static int GetGpTypeSize(GpType gpType)
        {
            switch (gpType)
            {
                case GpType.StringArray:
                    return 2;

                case GpType.Byte:
                    return 1;

                case GpType.Double:
                    return 8;

                case GpType.EventData:
                    return 3;

                case GpType.Float:
                    return 4;

                case GpType.Hashtable:
                    return 2;

                case GpType.Integer:
                    return 4;

                case GpType.Short:
                    return 2;

                case GpType.Long:
                    return 8;

                case GpType.IntegerArray:
                    return 4;

                case GpType.Boolean:
                    return 1;

                case GpType.OperationResponse:
                    return 3;

                case GpType.OperationRequest:
                    return 3;

                case GpType.String:
                    return 2;

                case GpType.Vector:
                    return 2;

                case GpType.ByteArray:
                    return 4;

                case GpType.Array:
                    return 4;

                case GpType.Null:
                    return 0;
            }
            return 1;
        }
    }
}
