using Cysharp.Threading.Tasks;
using Google.Protobuf;
using System;

namespace Framework.Network
{
    public class Connection : PacketHandler
    {
        public string ConnectionId { get; set; }
        
        private ServerSession session;
        public ServerSession Session 
        {
            get { return session; }
            set 
            { 
                session = value;
                session.connectedHandler += OnConnected;
                session.disconnectedHandler += OnDisConnected;
                session.receivedHandler += OnRecv;
            }
        }
        
        protected bool isConnected;
        protected Action connectedHandler;
        protected Action disconnectedHandler;

        private readonly PacketQueue packetQueue;
        private DateTime lastMessageSent;

        public Connection()
        {
            isConnected = false;

            packetQueue = new();
            lastMessageSent = new(1970, 1, 1);
        }

        private void OnConnected()
        {
            isConnected = true;

            AsyncPacketUpdate().Forget();
            HeartBeat().Forget();

            connectedHandler?.Invoke();
        }

        private void OnDisConnected()
        {
            isConnected = false;

            Session = null;

            disconnectedHandler?.Invoke();
        }

        private void OnRecv( ArraySegment<byte> buffer )
        {
            PacketManager.OnRecv(buffer, packetQueue);
        }

        public void Send( ArraySegment<byte> pkt )
        {
            if (!isConnected)
                return;
            
            lastMessageSent = DateTime.UtcNow;
            Session.Send(pkt);
        }

        public void Enter()
        {
            Protocol.C_ENTER enter = new();
            enter.ClientId = "Test";
            Send(PacketManager.MakeSendBuffer(enter));
        }

        public void Leave()
        {
            Protocol.C_LEAVE leave = new();
            Send(PacketManager.MakeSendBuffer(leave));
        }

        public void ReEnter()
        {
            Protocol.C_REENTER reEnter = new();
            reEnter.ClientId = "Test";
            Send(PacketManager.MakeSendBuffer(reEnter));
        }

        public void Disconnect()
        {
            Session.Disconnect();
        }

        public async UniTaskVoid AsyncPacketUpdate()
        {

            while (isConnected)
            {
                System.Collections.Generic.List<PacketMessage> packets = packetQueue.PopAll();

                for (int i = 0; i < packets.Count; i++)
                {
                    PacketMessage packet = packets[i];
                    _ = Handlers.TryGetValue(packet.Id, out Action<IMessage> handler);
                    handler?.Invoke(packet.Message);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.02f));
            }

            disconnectedHandler?.Invoke();
        }

        public async UniTaskVoid HeartBeat()
        {
            Protocol.C_HEARTBEAT heartbeat = new();
            ArraySegment<byte> heartbeatPkt = PacketManager.MakeSendBuffer(heartbeat);

            while (isConnected)
            {
                if ((long)(DateTime.UtcNow - lastMessageSent).TotalSeconds > 5)
                {
                    Send(heartbeatPkt);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
