using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using ExitGames.Logging;
using Photon.SocketServer.Diagnostics.Configuration;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer.Web
{
public class HttpApplicationHandler : IHttpAsyncHandler, IHttpHandler, IPhotonApplicationSink
{
    // Fields
    private static readonly ILogger log = LogManager.GetCurrentClassLogger();
    private readonly PeerCache peerCache;
    private readonly IPhotonApplication photonApplication;

    // Methods
    [CLSCompliant(false)]
    public HttpApplicationHandler(IPhotonApplication photonApplication, string applicationName) : this(photonApplication, applicationName, TimeSpan.FromSeconds(30.0))
    {
    }

    [CLSCompliant(false)]
    public HttpApplicationHandler(IPhotonApplication photonApplication, string applicationName, TimeSpan peerExpiration)
    {
        this.peerCache = new PeerCache();
        this.peerCache.PeerExpiration = peerExpiration;
        this.peerCache.OnPeerExpired += new Action<PhotonHttpPeer>(this.OnPeerCachePeerExpired);
        this.photonApplication = photonApplication;
        ((IPhotonControl) this.photonApplication).OnStart(string.Empty, applicationName, this, null, null, string.Empty);
        ((IPhotonControl) this.photonApplication).OnPhotonRunning();
    }

    public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
    {
        try
        {
            AsyncResult asyncCallback = new AsyncResult(cb, extraData);
            RequestState state = new RequestState(HttpContext.Current, asyncCallback);
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.ProcessRequestCallBack), state);
            return asyncCallback;
        }
        catch (Exception exception)
        {
            log.Error(exception);
            RpcHttpHelper.SendServerErrorResponse(context.Response);
            AsyncResult result2 = new AsyncResult(cb, extraData);
            result2.SetCompleted(null, true);
            return result2;
        }
    }

    [CLSCompliant(false)]
    public IPhotonPeer ConnectHixie76WebSocket(string ipAddress, ushort port, string url, string origin, object userData)
    {
        throw new NotImplementedException();
    }

    [CLSCompliant(false)]
    public IPhotonPeer ConnectWebSocket(string ipAddress, ushort port, WebSocketVersion version, string url, string subProtocols, object userData)
    {
        throw new NotImplementedException();
    }

    internal void DisconnectPeer(PhotonHttpPeer httpPeer)
    {
        this.photonApplication.OnDisconnect(httpPeer, httpPeer.GetUserData(), DisconnectReason.ManagedDisconnect, string.Empty, 0, 0, 0);
        this.peerCache.RemovePeer(httpPeer.PeerId);
    }

    public void EndProcessRequest(IAsyncResult result)
    {
    }

    private void ExecuteOnReceive(PhotonHttpPeer peer, byte[] data, int? invocationId)
    {
        if (log.IsDebugEnabled)
        {
            log.DebugFormat("Execute request: peerId={0}, invocationId={1}", new object[] { peer.PeerId, invocationId });
        }
        this.photonApplication.OnReceive(peer, peer.GetUserData(), data, MessageReliablity.Reliable, 0, MessageContentType.Binary, 0, 0, 0);
    }

    private void OnInitRequest(RequestState request)
    {
        try
        {
            Guid guid = Guid.NewGuid();
            string peerId = guid.ToString();
            if ((request.Context.Request.QueryString.Count < 1) || (request.Context.Request.QueryString[0] != "init"))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received empty peer id", new object[0]);
                }
                RpcHttpHelper.SendErrorResponse(request.Context.Response, "Invalid rpc request");
                request.AsyncResult.SetCompleted(null, false);
            }
            else
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received init request: pid={0}", new object[] { peerId });
                }
                PhotonHttpPeer photonPeer = new PhotonHttpPeer(peerId, this, request.Context);
                if (!this.peerCache.TryAddPeer(photonPeer))
                {
                    if (log.IsWarnEnabled)
                    {
                        log.WarnFormat("Failed to create peer: pid={0}", new object[] { peerId });
                    }
                    RpcHttpHelper.SendServerErrorResponse(request.Context.Response);
                }
                else
                {
                    byte[] data = RpcHttpHelper.ReadRequest(request.Context.Request);
                    this.photonApplication.OnInit(photonPeer, data, 1);
                    if (photonPeer.Queue.Count == 0)
                    {
                        request.AsyncResult.SetCompleted(null, false);
                    }
                    else
                    {
                        byte[] buffer = photonPeer.Queue.Dequeue();
                        request.Context.Response.OutputStream.Write(guid.ToByteArray(), 0, 0x10);
                        request.Context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        request.AsyncResult.SetCompleted(null, false);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            log.Error(exception);
        }
    }

    private void OnPeerCachePeerExpired(PhotonHttpPeer httpPeer)
    {
        if (log.IsDebugEnabled)
        {
            log.DebugFormat("Peer expired: id={0}", new object[] { httpPeer.PeerId });
        }
        this.photonApplication.OnDisconnect(httpPeer, httpPeer.GetUserData(), DisconnectReason.ManagedDisconnect, string.Empty, 0, 0, 0);
    }

    bool IPhotonApplicationSink.BroadcastEvent(IPhotonPeer[] peerList, byte[] data, MessageReliablity reliability, byte channelId, MessageContentType messageContentType, out SendResults[] results)
    {
        results = new SendResults[peerList.Length];
        for (int i = 0; i < peerList.Length; i++)
        {
            results[i] = peerList[i].Send(data, reliability, channelId, messageContentType);
        }
        return true;
    }

    IPhotonPeer IPhotonApplicationSink.Connect(string address, ushort port, object userData)
    {
        TcpPeer peer = new TcpPeer(this.photonApplication);
        peer.SetUserData(userData);
        peer.Connect(address, port);
        return peer;
    }

    IPhotonPeer IPhotonApplicationSink.ConnectENet(string ipAddress, ushort port, byte channelCount, object userData, object mtu)
    {
        throw new NotImplementedException();
    }

    IPhotonPeer IPhotonApplicationSink.ConnectMux(string ipAddress, ushort port, object userData)
    {
        throw new NotImplementedException();
    }

    public void ProcessRequest(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private void ProcessRequestCallBack(object state)
    {
        try
        {
            PhotonHttpPeer peer;
            RequestState request = (RequestState) state;
            HttpContext context = request.Context;
            string str = context.Request.Params.Get("pid");
            int maxHttpReceiveMessageSize = PhotonSettings.Default.MaxHttpReceiveMessageSize;
            if ((context.Request.ContentLength > maxHttpReceiveMessageSize) || (context.Request.InputStream.Length > maxHttpReceiveMessageSize))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Max message size exceeded: pid={0}, ContentLenght={1}", new object[] { str, context.Request.ContentLength });
                }
                RpcHttpHelper.SendResponse(context.Response, HttpStatusCode.BadRequest, "Max message size exceeded");
            }
            else if (string.IsNullOrEmpty(str))
            {
                this.OnInitRequest(request);
            }
            else if (!this.peerCache.TryGetPeer(request.Context, str, out peer))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received unknown peer id: {0}", new object[] { str });
                }
                RpcHttpHelper.SendResponse(context.Response, HttpStatusCode.Unauthorized, "Unknown peer id");
                request.AsyncResult.SetCompleted(null, false);
            }
            else
            {
                if (context.Request.RequestType == "POST")
                {
                    byte[] data = RpcHttpHelper.ReadRequest(context.Request);
                    if (data.Length == 1)
                    {
                        switch (data[0])
                        {
                            case 0:
                                this.SendQueuedMessages(peer, request);
                                request.AsyncResult.SetCompleted(null, false);
                                return;

                            case 1:
                                this.photonApplication.OnDisconnect(peer, peer.GetUserData(), DisconnectReason.ManagedDisconnect, string.Empty, 0, 0, 0);
                                this.peerCache.RemovePeer(peer.PeerId);
                                request.AsyncResult.SetCompleted(null, false);
                                return;
                        }
                    }
                    if (data.Length > 0)
                    {
                        string str2 = context.Request.Params["invocId"];
                        if (!string.IsNullOrEmpty(str2))
                        {
                            int invocationId;
                            if (!int.TryParse(str2, out invocationId))
                            {
                                log.DebugFormat("Received invalid invocation id: peerId={0}, invocationId={1}", new object[] { str, str2 });
                                RpcHttpHelper.SendResponse(context.Response, HttpStatusCode.BadRequest, "Invalid invocation id specified.");
                                request.AsyncResult.SetCompleted(null, false);
                                return;
                            }
                            peer.InvocationCache.Invoke(invocationId, () => this.ExecuteOnReceive(peer, data, new int?(invocationId)));
                        }
                        else
                        {
                            this.ExecuteOnReceive(peer, data, null);
                        }
                    }
                }
                this.SendQueuedMessages(peer, request);
                request.AsyncResult.SetCompleted(null, false);
            }
        }
        catch (Exception exception)
        {
            log.Error(exception);
        }
    }

    private void SendQueuedMessages(PhotonHttpPeer peer, RequestState request)
    {
        List<byte[]> list = peer.DequeueAll();
        if (list.Count > 0)
        {
            using (BinaryWriter writer = new BinaryWriter(request.Context.Response.OutputStream))
            {
                writer.Write((short) list.Count);
                foreach (byte[] buffer in list)
                {
                    writer.Write(buffer.Length);
                    writer.Write(buffer, 0, buffer.Length);
                }
                writer.Flush();
            }
        }
    }

    // Properties
    public bool IsReusable
    {
        get
        {
            return true;
        }
    }

    // Nested Types
    private class RequestState
    {
        // Fields
        public readonly AsyncResult AsyncResult;
        public readonly HttpContext Context;

        // Methods
        public RequestState(HttpContext context, AsyncResult asyncCallback)
        {
            this.Context = context;
            this.AsyncResult = asyncCallback;
        }
    }
}
}
