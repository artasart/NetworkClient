using Cysharp.Threading.Tasks;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using UnityEngine;

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
                session.disconnectedHandler += OnDisconnected;
                session.receivedHandler += OnRecv;
            }
        }

        protected ConnectionState state;

        protected Action connectedHandler;
        protected Action disconnectedHandler;

        private readonly PacketQueue packetQueue;

        private readonly Queue<long> pings;
        private long pingAverage;

        private long serverTime;
        public long calcuatedServerTime;
        private float delTime;

        ~Connection()
        {
            UnityEngine.Debug.Log("Connection Destructor");
        }

        public Connection()
        {
            state = ConnectionState.NORMAL;

            AddHandler(Handle_S_DISCONNECTED);
            AddHandler(Handle_S_PING);
            AddHandler(Handle_S_SERVERTIME);
            AddHandler(Handle_S_ENTER);

            packetQueue = new();
            pings = new();
            pingAverage = 0;

            serverTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

            AsyncPacketUpdate().Forget();
            Ping().Forget();
            UpdateServerTime().Forget();
        }

        private void OnConnected()
        {
            connectedHandler?.Invoke();
        }

        private void OnDisconnected()
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

            Session.Send(pkt);
        }

        private void Handle_S_ENTER( Protocol.S_ENTER pkt )
        {
            if (pkt.Result == "SUCCESS")
            {
                Protocol.C_SERVERTIME servertime = new();
                Send(PacketManager.MakeSendBuffer(servertime));
            }
        }

        private void Handle_S_DISCONNECTED( Protocol.S_DISCONNECT pkt )
        {
            UnityEngine.Debug.Log("Handle S Disconnect : " + pkt.Code);

            Close();
        }

        private void Handle_S_PING( Protocol.S_PING pkt )
        {
            long tick = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            long ping = tick - pkt.Tick;

            pings.Enqueue(ping);

            if (pings.Count > 5)
            {
                _ = pings.Dequeue();
            }

            long sum = 0;
            foreach (long item in pings)
            {
                sum += item;
            }

            pingAverage = sum / pings.Count;
        }

        private void Handle_S_SERVERTIME( Protocol.S_SERVERTIME pkt )
        {
            delTime = 0;
            serverTime = pkt.Tick + (pingAverage / 2);
            calcuatedServerTime = serverTime;
        }

        public void Close()
        {
            if (state == ConnectionState.CLOSED)
            {
                return;
            }

            state = ConnectionState.CLOSED;

            session?.RegisterDisconnect();
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

                await UniTask.Yield();
            }
        }

        public async UniTaskVoid Ping()
        {
            Protocol.C_PING ping = new();

            while (state == ConnectionState.NORMAL)
            {
                ping.Tick = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
                Send(PacketManager.MakeSendBuffer(ping));

                await UniTask.Delay(TimeSpan.FromSeconds(0.2));
            }
        }

        public async UniTaskVoid UpdateServerTime()
        {
            while (state == ConnectionState.NORMAL)
            {
                delTime += Time.deltaTime;
                calcuatedServerTime = serverTime + (long)Math.Round(delTime * 1000, 1);
                await UniTask.Yield();
            }
        }
    }
}
