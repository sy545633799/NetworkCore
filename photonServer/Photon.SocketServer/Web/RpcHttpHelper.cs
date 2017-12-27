using System.Net;
using System.Web;
using ExitGames.IO;
using ExitGames.Logging;

namespace Photon.SocketServer.Web
{
    internal static class RpcHttpHelper
    {
        // Fields
        private static readonly byte[] emptyByteArray = new byte[0];
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        // Methods
        internal static byte[] ReadRequest(HttpRequest httpRequest)
        {
            byte[] emptyByteArray = null;
            if (httpRequest.RequestType == "POST")
            {
                using (httpRequest.InputStream)
                {
                    return BinaryConverter.ConvertStreamToByteArray(httpRequest.InputStream, (int)httpRequest.InputStream.Length);
                }
            }
            if (httpRequest.QueryString.Count > 0)
            {
                emptyByteArray = HttpUtility.UrlDecodeToBytes(httpRequest.QueryString[httpRequest.QueryString.Count - 1]);
            }
            if (emptyByteArray == null)
            {
                emptyByteArray = RpcHttpHelper.emptyByteArray;
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("GET Request: Length={0}", emptyByteArray.Length);
            }
            return emptyByteArray;
        }

        internal static void SendErrorResponse(HttpResponse response, string message)
        {
            response.StatusCode = 400;
            response.StatusDescription = message;
            response.AddHeader("Cache-Control", "no-cache, must-revalidate, no-transform");
            response.Flush();
        }

        internal static void SendResponse(HttpResponse response, HttpStatusCode status, string message)
        {
            response.StatusCode = (int)status;
            response.StatusDescription = message;
            response.AddHeader("Cache-Control", "no-cache, must-revalidate, no-transform");
            response.Flush();
        }

        internal static void SendServerErrorResponse(HttpResponse response)
        {
            response.StatusCode = 500;
            response.StatusDescription = "Internal server error";
            response.AddHeader("Cache-Control", "no-cache, must-revalidate, no-transform");
        }
    }
}
