using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Wodsoft.Net.Service
{
    public class BinaryDataWriter
    {
        private MemoryStream Stream;

        public BinaryDataWriter()
        {
            Stream = new MemoryStream();
        }

        public void WriteBoolean(bool value)
        {
            if (value)
                Stream.WriteByte(1);
            else
                Stream.WriteByte(0);
        }

        public void WriteByte(byte value)
        {
            Stream.WriteByte(value);
        }

        public void WriteChar(char value)
        {
            Stream.Write(BitConverter.GetBytes(value), 0, 2);
        }

        public void WriteInt16(short value)
        {
            Stream.Write(BitConverter.GetBytes(value), 0, 2);
        }

        public void WriteInt32(int value)
        {
            Stream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void WriteInt64(long value)
        {
            Stream.Write(BitConverter.GetBytes(value), 0, 8);
        }

        public void WriteUInt16(ushort value)
        {
            Stream.Write(BitConverter.GetBytes(value), 0, 2);
        }

        public void WriteUInt32(uint value)
        {
            Stream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void WriteUInt64(ulong value)
        {
            Stream.Write(BitConverter.GetBytes(value), 0, 8);
        }

        public void WriteSingle(float value)
        {
            Stream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void WriteDouble(double value)
        {
            Stream.Write(BitConverter.GetBytes(value), 0, 8);
        }

        public void WriteString(string value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            Stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
            Stream.Write(data, 0, data.Length);
        }

        public void WriteBytes(byte[] value)
        {
            Stream.Write(BitConverter.GetBytes(value.Length), 0, 4);
            Stream.Write(value, 0, value.Length);
        }

        public void WriteGuid(Guid value)
        {
            Stream.Write(value.ToByteArray(), 0, 16);
        }

        public byte[] ToArray()
        {
            return Stream.ToArray();
        }
    }
}
