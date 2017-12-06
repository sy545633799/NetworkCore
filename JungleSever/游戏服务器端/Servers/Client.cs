using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Common;
using MySql.Data.MySqlClient;
using GameServer.Tool;
using GameServer.Model;
using GameServer.DAO;
namespace GameServer.Servers
{
    class Client
    {
        private Socket clientSocket;
        private Server server;
        private Message msg = new Message();
        private MySqlConnection mysqlConn;
        private Room room;
        private User user;
        private Result result;
        private ResultDAO resultDAO = new ResultDAO();

        public int HP
        {
            get;set;
        }
        public bool TakeDamage(int damage)
        {
            HP -= damage;
            HP = Math.Max(HP, 0);
            if (HP <= 0) return true;
            return false;
        }
        public bool IsDie()
        {
            return HP <= 0;
        }
        public MySqlConnection MySQLConn
        {
            get { return mysqlConn; }
        }
        public void SetUserData(User user,Result result)
        {
            this.user = user;
            this.result = result;
        }
        public string GetUserData()
        {
            return user.Id+","+ user.Username + "," + result.TotalCount + "," + result.WinCount;
        }
        public Room Room
        {
            set { room = value; }
            get { return room; }
        }
        public int GetUserId()
        {
            return user.Id;
        }

        public Client() { }
        public Client(Socket clientSocket,Server server)
        {
            this.clientSocket = clientSocket;
            this.server = server;
            mysqlConn = ConnHelper.Connect();
        }
        public void Start()
        {
            if (clientSocket == null || clientSocket.Connected == false) return;
            clientSocket.BeginReceive(msg.Data, msg.StartIndex, msg.RemainSize, SocketFlags.None, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if (clientSocket == null || clientSocket.Connected == false) return;
                int count = clientSocket.EndReceive(ar);
                if (count == 0)
                {
                    Close();
                }
                msg.ReadMessage(count,OnProcessMessage);
                Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Close();
            }
        }
        private void OnProcessMessage(RequestCode requestCode,ActionCode actionCode,string data)
        {
            server.HandleRequest(requestCode, actionCode, data, this);
        }
        private void Close()
        {
            ConnHelper.CloseConnection(mysqlConn);
            if (clientSocket != null)
                clientSocket.Close();
            if (room != null)
            {
                room.QuitRoom(this);
            }
            server.RemoveClient(this);
        }
        public void Send(ActionCode actionCode, string data)
        {
            try
            {
                byte[] bytes = Message.PackData(actionCode, data);
                clientSocket.Send(bytes);
            }catch(Exception e)
            {
                Console.WriteLine("无法发送消息:" + e);
            }
        }
        public bool IsHouseOwner()
        {
            return room.IsHouseOwner(this);
        }
        public void UpdateResult(bool isVictory)
        {
            UpdateResultToDB(isVictory);
            UpdateResultToClient();
        }
        private void UpdateResultToDB(bool isVictory)
        {
            result.TotalCount++;
            if (isVictory)
            {
                result.WinCount++;
            }
            resultDAO.UpdateOrAddResult(mysqlConn, result);
        }
        private void UpdateResultToClient()
        {
            Send(ActionCode.UpdateResult, string.Format("{0},{1}", result.TotalCount, result.WinCount));
        }
    }
}
