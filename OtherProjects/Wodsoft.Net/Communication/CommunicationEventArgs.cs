using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Communication
{
    public abstract class CommunicationEventArgs : EventArgs
    {
        public CommunicationEventArgs(ICommunication communication)
        {
            if (communication == null)
                throw new ArgumentNullException("communication");
            Communication = communication;
        }

        public ICommunication Communication { get; private set; }
    }

    public class CommunicationAcceptEventArgs : CommunicationEventArgs
    {
        public CommunicationAcceptEventArgs(ICommunication communication, byte[] head, Credential credential)
            : base(communication)
        {
            Head = head;
            Handled = false;
            Credential = credential;
        }

        public byte[] Head { get; private set; }

        public bool Handled { get; set; }

        public byte[] FailedData { get; set; }
        
        public Credential Credential { get; private set; }
    }

    public class CommunicationConnectEventArgs : CommunicationEventArgs
    {
        public CommunicationConnectEventArgs(ICommunication communication, bool success, byte[] failedData)
            : base(communication)
        {
            Success = success;
            FailedData = failedData;
        }

        public bool Success { get; private set; }

        public byte[] FailedData { get; private set; }
    }

    public class CommunicationReceiveEventArgs : CommunicationEventArgs
    {
        public CommunicationReceiveEventArgs(ICommunication communication, Guid dataID, int dataLength, byte[] head)
            : base(communication)
        {
            DataID = dataID;
            DataLength = dataLength;
            Head = head;
            Handled = false;
        }

        public Guid DataID { get; private set; }

        public int DataLength { get; private set; }

        public byte[] Head { get; private set; }

        public byte[] Data { get; internal set; }

        public bool Handled { get; set; }

        public bool Success { get; internal set; }

        public byte[] FailedData { get; set; }
    }

    public class CommunicationSendEventArgs : CommunicationEventArgs
    {
        public CommunicationSendEventArgs(ICommunication communication, byte[] data, byte[] head)
            : base(communication)
        {
            DataID = Guid.NewGuid();
            Data = data;
            Head = head;
        }

        public bool Handled { get; set; }

        public Guid DataID { get; private set; }

        public byte[] Head { get; private set; }

        public byte[] Data { get; private set; }

        public bool Success { get; internal set; }

        public byte[] FailedData { get; internal set; }
    }

    public class CommunicationDisconnectEventArgs : CommunicationEventArgs
    {
        public CommunicationDisconnectEventArgs(ICommunication communication, byte[] data)
            : base(communication)
        {
            Data = data;
        }

        public byte[] Data { get; private set; }
    }
}