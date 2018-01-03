using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace SenseConsoleApp
{
    public class SenseWebSocketClient
    {
        private ClientWebSocket _client;
        public Uri _senseServerURI;

        static async Task<string> GetDocList1()
        {
            var client = new SenseWebSocketClient(new Uri("ws://localhost:4848"));
            Console.WriteLine("Connecting to Qlik Sense...");
            Console.WriteLine("Getting document list...");
            var docs = await client.GetDocList();
            Console.WriteLine(docs);
            return docs;
        }

        public SenseWebSocketClient(Uri senseServerURI)
        {
            _client = new ClientWebSocket();
            _senseServerURI = senseServerURI;
        }
        public async Task<string> GetDocList()
        {
            string cmd = "{\"method\":\"GetDocList\",\"handle\":-1,\"params\":[],\"id\":7,\"jsonrpc\":\"2.0\"}";
            await _client.ConnectAsync(_senseServerURI, CancellationToken.None);
            await SendCommand(cmd);
            var docList = await Receive();
            return docList;
        }
        private async Task ConnectToSenseServer()
        {
            await _client.ConnectAsync(_senseServerURI, CancellationToken.None);
        }
        private async Task SendCommand(string jsonCmd)
        {
            ArraySegment<byte> outputBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonCmd));
            await _client.SendAsync(outputBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        private async Task<string> Receive()
        {
            var receiveBufferSize = 1536;
            byte[] buffer = new byte[receiveBufferSize];
            var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var resultJson = (new UTF8Encoding()).GetString(buffer);
            return resultJson;
        }
    }
}