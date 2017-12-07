using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wodsoft.Net.Communication;
using System.Threading;
using System.Reflection;

namespace Wodsoft.Net.Service
{
    public class ServiceHost
    {
        private Server Server;
        private ServiceChannelManager ChannelManager;
        private ChannelMessageFormatter MessageFormatter;
        private HostInstanceManager InstanceManager;
        private MessageIOManager IOManager;

        public ServiceHost(ServiceBinding binding)
        {
            Binding = binding;
            Server = new Server((ushort)binding.MainPort);
            Server.PreviewAccept += PreviewAccept;
            Server.AcceptCompleted += Accept;
            ChannelManager = new ServiceChannelManager();
            SetDataFormatter(new DataFormatter());            
            Unity = new ServiceUnity();
            InstanceManager = new HostInstanceManager(Unity);
            IOManager = new MessageIOManager();
        }

        public ServiceUnity Unity { get; private set; }

        public DataFormatter DataFormatter { get; private set; }

        public void SetDataFormatter(DataFormatter dataFormatter)
        {
            if (Opened)
                throw new InvalidOperationException("服务打开时不能更改。");
            if (dataFormatter == null)
                throw new ArgumentNullException("dataFormatter");
            DataFormatter = dataFormatter;
            MessageFormatter = new ChannelMessageFormatter(dataFormatter);
        }

        public ServiceBinding Binding { get; private set; }

        public bool Opened { get; private set; }

        public void Open()
        {
            if (Opened)
                throw new InvalidOperationException("服务已开始。");

            try
            {
                Server.Start();
            }
            catch
            {
                throw new InvalidOperationException("端口被占用。");
            }
            Opened = true;
        }

        public void Close()
        {
            if (!Opened)
                throw new InvalidOperationException("服务未开始。");
            Server.Stop();
            Opened = false;
        }

        public void RegisterChannel(ServiceChannel channel)
        {
            if (Opened)
                throw new InvalidOperationException("服务打开时不能注册服务频道。");
            if (channel.Provider.Mode == ServiceMode.Client)
                throw new NotSupportedException("服务器类型为客户端类型。");
            if (!ChannelManager.RegisterChannel(channel))
                throw new InvalidOperationException("服务频道名称重复。");
        }

        private void PreviewAccept(object sender, CommunicationAcceptEventArgs e)
        {
            //头信息认证
            if (e.Head == null)
            {
                e.Handled = true;
                return;
            }
            var head = Encoding.UTF8.GetString(e.Head).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (!head.Contains("ApplicationCommunicationFoundation"))
            {
                e.Handled = true;
                e.FailedData = new byte[] { 0 };
                return;
            }
            if (!head.Contains("Protocol=1.0"))
            {
                e.Handled = true;
                e.FailedData = new byte[] { 1 };
                return;
            }

            //是否需要认证
            if (Binding.SecurityMode != SecurityMode.Message && e.Credential == null)
            {
                e.Handled = true;
                if (Binding.SecurityMode == SecurityMode.MessageRequired)
                    e.FailedData = new byte[] { 2 };
                else if (Binding.SecurityMode == SecurityMode.Windows)
                    e.FailedData = new byte[] { 3 };
                return;
            }

            Credential credential = null;
            if (e.Credential != null)
            {
                if (Binding.SecurityMode != SecurityMode.Windows)
                {
                    credential = new MessageCredential(Encoding.UTF8.GetString(e.Credential.Username), Encoding.UTF8.GetString(e.Credential.Password));
                }
                else
                {

                }
            }
            ServiceUser user = new ServiceUser((ServerClient)e.Communication, credential);
            e.Communication["user"] = user;
        }

        private void Accept(object sender, CommunicationAcceptEventArgs e)
        {
            e.Communication.PreviewReceive += PreviewReceive;
            e.Communication.ReceiveCompleted += Receive;
            e.Communication.DisconnectCompleted += Disconnect;
            ServiceUser user = (ServiceUser)e.Communication["user"];
            InstanceManager.AddUser(user);
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
            switch (e.Head[0])
            {
                case 0:
                    Execute_0_ChannelExist(e);
                    break;
                case 1:
                    Execute_1_CreateChannel(e);
                    break;
                case 2:
                    Execute_2_MethodInvoke(e);
                    break;
                case 3:
                    var id = new Guid(e.Head.Skip(1).Take(16).ToArray());
                    IOManager.SetMessage(id, e.Data);
                    break;
            }
        }

        private void Disconnect(object sender, CommunicationDisconnectEventArgs e)
        {
            ServiceUser user = (ServiceUser)e.Communication["user"];
            InstanceManager.RemoveUser(user);
            user.Close();
        }

        private void Execute_0_ChannelExist(CommunicationReceiveEventArgs e)
        {
            string channelName = MessageFormatter.ToChannelExist(e.Data);
            if (ChannelManager.Exist(channelName))
                e.Communication.Send(new byte[] { 1 }, e.Head);
            else
                e.Communication.Send(new byte[] { 0 }, e.Head);
        }

        private void Execute_1_CreateChannel(CommunicationReceiveEventArgs e)
        {
            string channelName = MessageFormatter.ToChannelExist(e.Data);
            if (ChannelManager.Exist(channelName))
            {
                ServiceUser user = (ServiceUser)e.Communication["user"];
                InstanceManager.GetInstance(ChannelManager[channelName], InvokeMethod, user);
                e.Communication.Send(new byte[] { 1 }, e.Head);
            }
            else
                e.Communication.Send(new byte[] { 0 }, e.Head);
        }

        private void Execute_2_MethodInvoke(CommunicationReceiveEventArgs e)
        {
            ServiceChannel channel;
            int methodIndex;
            object[] args;
            MessageFormatter.ToChannelMethodInvoke(e.Data, ChannelManager, out channel, out methodIndex, out args);

            ServiceUser user = (ServiceUser)e.Communication["user"];
            byte[] data;
            if (channel == null)
            {
                data = MessageFormatter.FromChannelMethodResult(null, "ChannelNotExist");
                e.Communication.Send(data, e.Head);
                return;
            }

            if (methodIndex >= channel.Provider.ServerOperations.Count)
            {
                data = MessageFormatter.FromChannelMethodResult(null, "MethodNotExist");
                e.Communication.Send(data, e.Head);
                return;
            }
            var method = channel.Provider.ServerOperations[methodIndex];

            var instance = InstanceManager.GetInstance(channel, InvokeMethod, user);
            ServiceContext.Context = user.GetContext(channel);

            if (instance == null)
            {
                data = MessageFormatter.FromChannelMethodResult(null, "InstanceCreateFailed");
                e.Communication.Send(data, e.Head);
                return;
            }
            object result;
            if (args.Length == 0)
                args = null;
            try
            {
                result = method.Invoke(instance, args);
            }
            catch (ArgumentException ex)
            {
                data = MessageFormatter.FromChannelMethodResult(null, "MethodParametersError");
                e.Communication.Send(data, e.Head);
                return;
            }
            catch (Exception ex)
            {
                data = MessageFormatter.FromChannelMethodResult(null, "MethodRuntimeError");
                e.Communication.Send(data, e.Head);
                return;
            }
            data = MessageFormatter.FromChannelMethodResult(result, null);
            e.Communication.Send(data, e.Head);
        }

        internal object InvokeMethod(ServiceChannel channel, MethodInfo method, object[] args)
        {
            return InvokeMethod(channel.Name, method.Name, channel.Provider.ClientOperations.IndexOf(method), args, method.ReturnType);
        }

        internal object InvokeMethod(string channelName, string methodName, int methodIndex, object[] args, Type returnType)
        {
            var data = MessageFormatter.FromChannelMethodInvoke(channelName, methodIndex, args);
            var head = new byte[17];
            head[0] = 3;
            var id = Guid.NewGuid();
            id.ToByteArray().CopyTo(head, 1);
            ServiceContext.Current.User.Client.Send(data, head);
            IOManager.BeginMessage(id);
            object result;
            string errorMsg;
            MessageFormatter.ToChannelMethodResult(IOManager.EndMessage(id), returnType, out result, out errorMsg);
            if (errorMsg == null)
                return result;

            switch (errorMsg)
            {
                case "ChannelNotExist":
                    throw new ChannelNotExistException(channelName, ServiceContext.Current.User.Client.EndPoint);
                case "MethodNotExist":
                    throw new MethodNotExistException(channelName, methodName, ServiceContext.Current.User.Client.EndPoint);
                case "MethodParametersError":
                    throw new MethodParamaterException(channelName, methodName, ServiceContext.Current.User.Client.EndPoint, args);
                case "MethodRuntimeError":
                    throw new MethodRuntimeException(channelName, methodName, ServiceContext.Current.User.Client.EndPoint);
            }
            return null;
        }
    }
}
