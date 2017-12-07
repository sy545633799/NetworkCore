using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wodsoft.Net.Communication;
using System.Reflection;

namespace Wodsoft.Net.Service
{
    internal class ChannelMessageFormatter
    {
        private DataFormatter Formatter;

        public ChannelMessageFormatter(DataFormatter dataFormatter)
        {
            Formatter = new DataFormatter();
        }

        public byte[] FromChannelExist(string channelName)
        {
            return Encoding.UTF8.GetBytes(channelName);
        }

        public string ToChannelExist(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public byte[] FromChannelMethodInvoke(string channelName, int methodIndex, params object[] args)
        {
            BinaryDataWriter writer = new BinaryDataWriter();
            writer.WriteString(channelName);
            writer.WriteInt32(methodIndex);
            writer.WriteByte((byte)args.Length);
            foreach (var arg in args)
                writer.WriteBytes(Formatter.Serialize(arg));
            return writer.ToArray();
        }

        public void ToChannelMethodInvoke(byte[] data, ServiceChannelManager channelManager, out ServiceChannel channel, out int methodIndex, out object[] args)
        {
            BinaryDataReader reader = new BinaryDataReader(data);
            channel = channelManager[reader.ReadString()];
            if (channel == null)
            {
                methodIndex = -1;
                args = null;
                return;
            }
            methodIndex = reader.ReadInt32();
            if (methodIndex < 0 || methodIndex >= channel.Provider.ServerOperations.Count)
            {
                methodIndex = -1;
                args = null;
                return;
            }
            var method = channel.Provider.ServerOperations[methodIndex];
            var parameter = method.GetParameters();
            args = new object[reader.ReadByte()];
            for (int i = 0; i < args.Length; i++)
                args[i] = Formatter.Deserialize(parameter[i].ParameterType, reader.ReadBytes());
        }

        public byte[] FromChannelMethodResult(object result, string errorMsg)
        {
            BinaryDataWriter writer = new BinaryDataWriter();
            if (errorMsg == null)
            {
                writer.WriteBoolean(true);
                writer.WriteBytes(Formatter.Serialize(result));
            }
            else
            {
                writer.WriteBoolean(false);
                writer.WriteString(errorMsg);
            }
            return writer.ToArray();
        }

        public void ToChannelMethodResult(byte[] data, Type resultType, out object result, out string errorMsg)
        {
            BinaryDataReader reader = new BinaryDataReader(data);
            if (reader.ReadBoolean())
            {
                errorMsg = null;
                if (resultType == null || resultType == typeof(void))
                    result = null;
                else
                    result = Formatter.Deserialize(resultType, reader.ReadBytes());
            }
            else
            {
                result = null;
                errorMsg = reader.ReadString();
            }
        }
    }
}
