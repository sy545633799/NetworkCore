using System.Runtime.Remoting.Messaging;
using System.Web;

namespace Photon.SocketServer.Web
{
    internal class AsyncHttpRequest
    {
        // Fields
        public readonly AsyncResult AsyncResult;
        public readonly HttpContext Context;

        // Methods
        public AsyncHttpRequest(HttpContext context, AsyncResult asyncCallback)
        {
            this.Context = context;
            this.AsyncResult = asyncCallback;
        }
    }
}
