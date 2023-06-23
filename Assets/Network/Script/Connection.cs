using Cysharp.Threading.Tasks;
using Google.Protobuf;
using System;

namespace Framework.Network
{
    public enum ConnectionState
    {
        NORMAL,
        CLOSED
    }

    public class Connection : PacketHandler
    {
        public string ConnectionId { get; set; }

        private ServerSession session;
        public ServerSession Session
        {
            get => session;
            set
            {
                session = value;
                session.connectedHandler += OnConnected;
                session.disconnectedHandler += OnDisConnected;
                session.receivedHandler += OnRecv;
            }
        }

        protected ConnectionState state;

        protected Action connectedHandler;
        protected Action disconnectedHandler;

        private readonly PacketQueue packetQueue;
        private DateTime lastMessageSent;

        ~Connection()
        {
            UnityEngine.Debug.Log("Connection Destructor");
        }

        public Connection()
        {
            state = ConnectionState.NORMAL;

            AddHandler(Handle_S_DISCONNECTED);

            packetQueue = new();
            lastMessageSent = new(1970, 1, 1);

            AsyncPacketUpdate().Forget();
            HeartBeat().Forget();
        }

        private void OnConnected()
        {
            connectedHandler?.Invoke();
        }

        private void OnDisConnected()
        {
            disconnectedHandler?.Invoke();
        }

        private void OnRecv( ArraySegment<byte> buffer )
        {
            PacketManager.OnRecv(buffer, packetQueue);
        }

        public void Send( ArraySegment<byte> pkt )
        {
            if (session == null)
            {
                return;
            }

            lastMessageSent = DateTime.UtcNow;
            Session.Send(pkt);
        }

        public void Enter()
        {
            Protocol.C_ENTER enter = new()
            {
                ClientId = "Test"
            };
            Send(PacketManager.MakeSendBuffer(enter));
        }

        public void Leave()
        {
            Protocol.C_LEAVE leave = new();
            Send(PacketManager.MakeSendBuffer(leave));
        }

        public void ReEnter()
        {
            Protocol.C_REENTER reEnter = new()
            {
                ClientId = "Test"
            };
            Send(PacketManager.MakeSendBuffer(reEnter));
        }

        //테스트용. 일반적인 상황에서는 사용할 일 없음
        public void Disconnect()
        {
            session.Disconnect();
        }

        private void Handle_S_DISCONNECTED( Protocol.S_DISCONNECT pkt )
        {
            UnityEngine.Debug.Log("Connection Disconnected : " + ConnectionId + ", " + pkt.Code);
            Close();
        }

        public void Close()
        {
            if (state == ConnectionState.CLOSED)
            {
                return;
            }

            state = ConnectionState.CLOSED;

            if(session != null)
            {
                session.Disconnect();
            }
        }

        public async UniTaskVoid AsyncPacketUpdate()
        {
            while (state == ConnectionState.NORMAL || !packetQueue.Empty())
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
        }

        public async UniTaskVoid HeartBeat()
        {
            Protocol.C_HEARTBEAT heartbeat = new();
            ArraySegment<byte> heartbeatPkt = PacketManager.MakeSendBuffer(heartbeat);

            while (state == ConnectionState.NORMAL)
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
