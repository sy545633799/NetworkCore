using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Wodsoft.Net.Communication;
using System.Threading;
using System.Reflection;

namespace Wodsoft.Net.Service
{
    public class ServiceClient
    {
        private Client Client;
        private IPEndPoint RemoteEndPoint;
        private AutoResetEvent ConnectAutoReset;
        private MessageIOManager IOManager;
        private ChannelMessageFormatter MessageFormatter;
        internal Dictionary<ServiceChannel, object> ChannelBuffer;
        private InstanceTypeBuilder Builder;
        private static byte[] Head = Encoding.UTF8.GetBytes("ApplicationCommunicationFoundation;Protocol=1.0");

        public ServiceClient(IPEndPoint remoteEndPoint)
        {
            ChannelBuffer = new Dictionary<ServiceChannel, object>();
            Client = new Communication.Client();
            Client.ConnectCompleted += Connect;
            Client.PreviewReceive += PreviewReceive;
            Client.ReceiveCompleted += Receive;
            Client.DisconnectCompleted += Disconnect;
            RemoteEndPoint = remoteEndPoint;
            Connected = false;
            IOManager = new MessageIOManager();
            DataFormatter = new DataFormatter();
            Security = new SecurityManager();
            Unity = new ServiceUnity();
            Builder = new InstanceTypeBuilder(OperationMode.Client);
        }

        private DataFormatter _DataFormatter;
        public DataFormatter DataFormatter
        {
            get
            {
                return _DataFormatter;
            }
            set
            {
                if (Connected)
                    throw new InvalidOperationException("服务连接时不能更改。");
                if (value == null)
                    throw new ArgumentNullException("DataFormatter");
                _DataFormatter = value;
                MessageFormatter = new ChannelMessageFormatter(value);
            }
        }

        public ServiceUnity Unity { get; private set; }

        public SecurityManager Security { get; private set; }

        public bool Connected { get; private set; }

        public void Connect()
        {
            Client.Connect(RemoteEndPoint, Head);
            ConnectAutoReset = new AutoResetEvent(false);
            ConnectAutoReset.WaitOne();
            if (!Connected)
            {
                if (ConnectFailedCode == 1)
                    throw new ConnectFailedException("协议版本不相同。");
                else if (ConnectFailedCode == 2)
                    throw new ConnectFailedException("缺少Message认证信息。");
                else if (ConnectFailedCode == 3)
                    throw new ConnectFailedException("缺少Windows认证信息。");
                else
                    throw new ConnectFailedException("未知错误。");
            }
        }

        public void Disconnect()
        {
            Client.Disconnect();
        }

        public int MessageTimeout { get { return IOManager.Timeout; } set { IOManager.Timeout = value; } }

        public ChannelFactory<T> GetChannelFactory<T>(ServiceChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");
            if (channel.Provider.Mode != ServiceMode.Client)
                throw new NotSupportedException("服务类型不是客户端类型。");
            if (typeof(T) != channel.Provider.Contract)
                throw new ArgumentException("T类型与服务契约类型不一样。");
            if (!ChannelBuffer.ContainsKey(channel))
                ChannelBuffer.Add(channel, new ChannelFactory<T>(this, channel));
            return (ChannelFactory<T>)ChannelBuffer[channel];
        }

        private int ConnectFailedCode;
        private void Connect(object sender, CommunicationConnectEventArgs e)
        {
            Connected = e.Success;
            if (!e.Success)
            {
                ConnectFailedCode = e.FailedData[0];
            }
            ConnectAutoReset.Set();
        }

        private void PreviewReceive(object sender, CommunicationReceiveEventArgs e)
        {
            if (e.Head == null || e.Head.Length != 17)
            {
                e.Handled = true;
                return;
            }
        }

        private void Receive(object sender, CommunicationReceiveEventArgs e)
        {
            if (e.Head[0] < 3)
            {
                var id = new Guid(e.Head.Skip(1).Take(16).ToArray());
                IOManager.SetMessage(id, e.Data);
                return;
            }
            Receive_3_InvokeMethod(e);
        }

        private void Disconnect(object sender, CommunicationDisconnectEventArgs e)
        {
            Connected = false;
        }

        internal bool InvokeChannelExist(string channelName)
        {
            var data = MessageFormatter.FromChannelExist(channelName);
            var head = new byte[17];
            head[0] = 0;
            var id = Guid.NewGuid();
            id.ToByteArray().CopyTo(head, 1);
            bool completed = false;
            while (!completed)
                try
                {
                    Client.Send(data, head);
                    IOManager.BeginMessage(id);
                    completed = true;
                }
                catch
                {

                }
            return IOManager.EndMessage(id)[0] == 1;
        }

        internal void InvokeCreateChannel(string channelName)
        {
            var data = MessageFormatter.FromChannelExist(channelName);
            var head = new byte[17];
            head[0] = 1;
            var id = Guid.NewGuid();
            id.ToByteArray().CopyTo(head, 1);
            Client.Send(data, head);
            IOManager.BeginMessage(id);
            IOManager.EndMessage(id);
        }

        internal void Receive_3_InvokeMethod(CommunicationReceiveEventArgs e)
        {
            byte[] data;
            ServiceChannel channel;
            int methodIndex;
            object[] args;
            BinaryDataReader reader = new BinaryDataReader(e.Data);
            var channelName = reader.ReadString().ToLower().Trim();
            channel = ChannelBuffer.Keys.SingleOrDefault(t => t.Name.ToLower().Trim() == channelName);
            if (channel == null)
            {
                data = MessageFormatter.FromChannelMethodResult(null, "ChannelNotExist");
                Client.Send(data, e.Head);
                return;
            }
            methodIndex = reader.ReadInt32();
            if (methodIndex >= channel.Provider.ClientOperations.Count)
            {
                data = MessageFormatter.FromChannelMethodResult(null, "MethodNotExist");
                Client.Send(data, e.Head);
                return;
            }
            var method = channel.Provider.ClientOperations[methodIndex];
            var parameter = method.GetParameters();
            args = new object[reader.ReadByte()];
            for (int i = 0; i < args.Length; i++)
                if (channel.Formatter != null)
                    args[i] = channel.Formatter.Deserialize(parameter[i].ParameterType, reader.ReadBytes());
                else
                    args[i] = DataFormatter.Deserialize(parameter[i].ParameterType, reader.ReadBytes());

            var instance = ChannelBuffer[channel].GetType().GetMethod("GetChannel").Invoke(ChannelBuffer[channel], null);

            if (instance == null)
            {
                data = MessageFormatter.FromChannelMethodResult(null, "InstanceCreateFailed");
                Client.Send(data, e.Head);
                return;
            }
            object result;
            try
            {
                result = method.Invoke(instance, args);
            }
            catch (ArgumentException)
            {
                data = MessageFormatter.FromChannelMethodResult(null, "MethodParametersError");
                Client.Send(data, e.Head);
                return;
            }
            catch (Exception)
            {
                data = MessageFormatter.FromChannelMethodResult(null, "MethodRuntimeError");
                Client.Send(data, e.Head);
                return;
            }
            data = MessageFormatter.FromChannelMethodResult(result, null);
            Client.Send(data, e.Head);
        }

        internal object InvokeMethod(ServiceChannel channel, MethodInfo method, object[] args)
        {
            return InvokeMethod(channel.Name, method.Name, channel.Provider.ServerOperations.IndexOf(method), args, method.ReturnType);
        }

        internal object InvokeMethod(string channelName, string methodName, int methodIndex, object[] args, Type returnType)
        {
            var data = MessageFormatter.FromChannelMethodInvoke(channelName, methodIndex, args);
            var head = new byte[17];
            head[0] = 2;
            var id = Guid.NewGuid();
            id.ToByteArray().CopyTo(head, 1);
            Client.Send(data, head);
            IOManager.BeginMessage(id);
            object result;
            string errorMsg;
            MessageFormatter.ToChannelMethodResult(IOManager.EndMessage(id), returnType, out result, out errorMsg);
            if (errorMsg == null)
                return result;

            switch (errorMsg)
            {
                case "ChannelNotExist":
                    throw new ChannelNotExistException(channelName, RemoteEndPoint);
                case "MethodNotExist":
                    throw new MethodNotExistException(channelName, methodName, RemoteEndPoint);
                case "MethodParametersError":
                    throw new MethodParamaterException(channelName, methodName, RemoteEndPoint, args);
                case "MethodRuntimeError":
                    throw new MethodRuntimeException(channelName, methodName, RemoteEndPoint);
            }
            return null;
        }

        internal object CreateInstance(ServiceChannel channel)
        {
            Type type;
            //try
            //{
            type = Builder.GetInstanceType(channel.Provider.Instance, channel.Provider.Contract);
            //}
            //catch
            //{
            //    return null;
            //}

            InstanceProxy proxy = new InstanceProxy(channel, channel.Provider.ServerOperations.ToList(), InvokeMethod);

            if (Unity.IsRegistered(type))
                return Unity.GetInstance(type);
            foreach (var constructor in type.GetConstructors())
            {
                var parameters = constructor.GetParameters();
                object[] parameterArray = new object[parameters.Length];
                bool parameterSuccess = true;
                parameterArray[0] = proxy;
                for (int i = 1; i < parameters.Length; i++)
                {
                    object obj = Unity.GetInstance(parameters[i].ParameterType);
                    if (obj == null)
                    {
                        parameterSuccess = false;
                        break;
                    }
                    parameterArray[i] = obj;
                }
                if (!parameterSuccess)
                    continue;
                try
                {
                    return Activator.CreateInstance(type, parameterArray);
                }
                catch
                {

                }
            }
            return null;
        }
    }
}
