using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Wodsoft.Net.Service
{
    public class BinaryDataReader
    {
        private MemoryStream Stream;

        public BinaryDataReader(byte[] data)
        {
            Stream = new MemoryStream(data);
        }

        private void CheckEndOfStream()
        {
            if (IsEndOfData)
                throw new InvalidOperationException("超出数据范围。");
        }

        public bool IsEndOfData
        {
            get
            {
                return Stream.Position == Stream.Length;
            }
        }

        public bool ReadBoolean()
        {
            CheckEndOfStream();
            int value = Stream.ReadByte();
            if (value == 1)
                return true;
            else if (value == 0)
                return false;
            else
                throw new InvalidDataException("读取布尔值失败。");
        }

        public byte ReadByte()
        {
            CheckEndOfStream();
            return (byte)Stream.ReadByte();
        }

        public char ReadChar()
        {
            CheckEndOfStream();
            byte[] data = new byte[2];
            Stream.Read(data, 0, data.Length);
            return BitConverter.ToChar(data, 0);
        }

        public short ReadInt16()
        {
            CheckEndOfStream();
            byte[] data = new byte[2];
            Stream.Read(data, 0, data.Length);
            return BitConverter.ToInt16(data, 0);
        }

        public int ReadInt32()
        {
            CheckEndOfStream();
            byte[] data = new byte[4];
            Stream.Read(data, 0, data.Length);
            return BitConverter.ToInt32(data, 0);
        }

        public long ReadInt64()
        {
            CheckEndOfStream();
            byte[] data = new byte[8];
            Stream.Read(data, 0, data.Length);
            return BitConverter.ToInt64(data, 0);
        }

        public ushort ReadUInt16()
        {
            CheckEndOfStream();
            byte[] data = new byte[2];
            Stream.Read(data, 0, data.Length);
            return BitConverter.ToUInt16(data, 0);
        }

        public uint ReadUInt32()
        {
            CheckEndOfStream();
            byte[] data = new byte[4];
            Stream.Read(data, 0, data.Length);
            return BitConverter.ToUInt32(data, 0);
        }

        public ulong ReadUInt64()
        {
            CheckEndOfStream();
            byte[] data = new byte[8];
            Stream.Read(data, 0, data.Length);
            return BitConverter.ToUInt64(data, 0);
        }

        public float ReadSingle()
        {
            CheckEndOfStream();
            byte[] data = new byte[4];
            Stream.Read(data, 0, data.Length);
            return BitConverter.ToSingle(data, 0);
        }

        public double ReadDouble()
        {
            CheckEndOfStream();
            byte[] data = new byte[8];
            Stream.Read(data, 0, data.Length);
            return BitConverter.ToDouble(data, 0);
        }

        public string ReadString()
        {
            CheckEndOfStream();
            byte[] length = new byte[4];
            Stream.Read(length, 0, 4);
            byte[] data = new byte[BitConverter.ToInt32(length, 0)];
            Stream.Read(data, 0, data.Length);
            return Encoding.UTF8.GetString(data);
        }

        public byte[] ReadBytes()
        {
            CheckEndOfStream();
            byte[] length = new byte[4];
            Stream.Read(length, 0, 4);
            byte[] data = new byte[BitConverter.ToInt32(length, 0)];
            Stream.Read(data, 0, data.Length);
            return data;
        }

        public Guid ReadGuid()
        {
            CheckEndOfStream();
            byte[] data = new byte[16];
            Stream.Read(data, 0, data.Length);
            return new Guid(data);
        }
    }
}
