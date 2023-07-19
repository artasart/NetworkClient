using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Google.Protobuf;
using Protocol;
using System;
using System.Collections;
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
                session.disconnectedHandler += OnDisConnected;
                session.receivedHandler += OnRecv;
            }
        }

        protected ConnectionState state;

        protected Action connectedHandler;
        protected Action disconnectedHandler;

        private readonly PacketQueue packetQueue;
        private DateTime lastMessageSent;

        private Queue<long> pings;
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

            packetQueue = new();
            lastMessageSent = new(1970, 1, 1);
            pings = new();

            serverTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

            AsyncPacketUpdate().Forget();
            //HeartBeat().Forget();
            //Ping Works as HeartBeat
            Ping().Forget();
            UpdateServerTime().Forget();
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
                pings.Dequeue();
            }

            long sum = 0;
            foreach (var item in pings)
            {
                sum += item;
            }

            pingAverage = sum / pings.Count;

            //UnityEngine.Debug.Log("Ping : " + pingAverage);
        }

        private void Handle_S_SERVERTIME( Protocol.S_SERVERTIME pkt )
        {
            //Debug.Log("Time Diff : " + (pkt.Tick + pingAverage / 2 - serverTime));

            delTime = 0;
            serverTime = pkt.Tick + pingAverage/2;

            Debug.Log("Time Diff : " + (calcuatedServerTime - serverTime));

            calcuatedServerTime = serverTime;
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

                await UniTask.Yield();
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
