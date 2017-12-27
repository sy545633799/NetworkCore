using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    public class MyApplication : ApplicationBase
    {
        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            return null;
        }

        protected override void Setup()
        {
           
        }

        protected override void TearDown()
        {
            
        }
    }
}
