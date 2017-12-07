using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Wodsoft.Net.Sockets
{
    public class UDPNatServer
    {
        UDPService udp;
        Dictionary<Guid, IPEndPoint> clients;
        List<Guid> completed;

        public UDPNatServer(int port)
        {
            clients = new Dictionary<Guid, IPEndPoint>();
            completed = new List<Guid>();
            udp = new UDPService();
            udp.Port = port;
            udp.ReceiveCompleted += Received;
        }

        public void Start()
        {
            udp.Start();
        }

        public void Stop()
        {
            udp.Stop();
        }

        private void Received(object sender, UDPServiceEventArgs e)
        {
            if (e.Data.Length != 32)
                return;

            Guid RequestID = new Guid(e.Data.Take(16).ToArray());
            if (!clients.ContainsKey(RequestID))
                clients.Add(RequestID, e.EndPoint);

            Guid ResponseID = new Guid(e.Data.Skip(16).ToArray());
            if (clients.ContainsKey(ResponseID))
            {
                List<byte> data = new List<byte>();
                data.AddRange(clients[ResponseID].Address.GetAddressBytes());
                data.AddRange(BitConverter.GetBytes(clients[ResponseID].Port));
                udp.Send(e.EndPoint, data.ToArray());
                if (completed.Contains(ResponseID))
                {
                    completed.Remove(ResponseID);
                    clients.Remove(ResponseID);
                    clients.Remove(RequestID);
                }
                else
                    completed.Add(RequestID);
            }
            else
            {
                udp.Send(e.EndPoint, new byte[1]);
            }
        }
    }

}
