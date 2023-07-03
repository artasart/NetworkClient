using System;
using System.Net;

namespace Framework.Network
{
    public class ServerSession : PacketSession
    {
        public Action connectedHandler;
        public Action disconnectedHandler;
        public Action<ArraySegment<byte>> receivedHandler;

        public override void OnConnected( EndPoint endPoint )
        {
            connectedHandler?.Invoke();
        }

        public override void OnDisconnected( EndPoint endPoint )
        {
            disconnectedHandler?.Invoke();
        }

        public override void OnRecvPacket( ArraySegment<byte> buffer )
        {
            receivedHandler?.Invoke(buffer);
        }

        public override void OnSend( int numOfBytes ) { }
    }
}