using System;
using System.Net;

namespace Framework.Network
{
    public class ServerSession : PacketSession
    {
        public Action connectedHandler;
        public Action disconnectedHandler;
        public Action<ArraySegment<byte>> receivedHandler;

        public override void OnConnected( EndPoint _endPoint )
        {
            connectedHandler?.Invoke();
        }

        public override void OnDisconnected( EndPoint _endPoint )
        {
            disconnectedHandler?.Invoke();
        }

        public override void OnRecvPacket( ArraySegment<byte> buffer )
        {
            receivedHandler?.Invoke(buffer);
        }

        public override void OnSend( int _numOfBytes ) { }
    }
}