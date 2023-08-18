using Cysharp.Threading.Tasks;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Threading;
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

        protected ServerSession session;
        public ServerSession Session
        {
            get => session;
            set
            {
                session = value;
                session.connectedHandler += _OnConnected;
                session.disconnectedHandler += _OnDisconnected;
                session.receivedHandler += _OnRecv;
            }
        }

        protected ConnectionState state;

        protected Action connectedHandler;
        protected Action disconnectedHandler;

        public PacketQueue PacketQueue { get; }

        private readonly Queue<long> pings;
        public long pingAverage;

        private long serverTime;
        public long calcuatedServerTime;
        private float delTime;
        readonly System.Diagnostics.Stopwatch stopwatch;

        protected CancellationTokenSource cts;

        ~Connection()
        {
            UnityEngine.Debug.Log("Connection Destructor");
        }

        public Connection()
        {
            state = ConnectionState.NORMAL;

            AddHandler(Handle_S_DISCONNECTED);
            AddHandler(Handle_S_ENTER);

            PacketQueue = new();
            pings = new();
            pingAverage = 0;

            serverTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            calcuatedServerTime = serverTime;
            delTime = 0f;

            stopwatch = new();

            cts = new();

            Ping(cts).Forget();
            UpdateServerTime(cts).Forget();
            //순서 중요, UpdateServerTime 이후에 PacketUpdate 실행
            AsyncPacketUpdate(cts).Forget();
        }

        private void _OnConnected()
        {
            connectedHandler?.Invoke();
        }

        private void _OnDisconnected()
        {
            disconnectedHandler?.Invoke();
        }

        protected void _OnRecv( ArraySegment<byte> buffer )
        {
            PacketManager.OnRecv(buffer, this);
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

        public void Handle_S_PING( Protocol.S_PING pkt )
        {
            pings.Enqueue((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds - pkt.Tick);

            if (pings.Count > 10)
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

        public void Handle_S_SERVERTIME( Protocol.S_SERVERTIME pkt )
        {
            serverTime = pkt.Tick + (pingAverage / 2);

            stopwatch.Start();
        }

        public void Close()
        {
            if (state == ConnectionState.CLOSED)
            {
                return;
            }

            state = ConnectionState.CLOSED;

            ConnectionManager.RemoveConnection(this);

            session?.RegisterDisconnect();
        }

        public async UniTaskVoid Ping( CancellationTokenSource cts )
        {
            Protocol.C_PING ping = new();

            while (!cts.IsCancellationRequested && state == ConnectionState.NORMAL)
            {
                ping.Tick = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
                Send(PacketManager.MakeSendBuffer(ping));

                await UniTask.Delay(TimeSpan.FromSeconds(0.2));
            }
        }

        public async UniTaskVoid AsyncPacketUpdate( CancellationTokenSource cts )
        {
            while ((!cts.IsCancellationRequested && state == ConnectionState.NORMAL) || !PacketQueue.Empty())
            {
                System.Collections.Generic.List<PacketMessage> packets = PacketQueue.PopAll();

                for (int i = 0; i < packets.Count; i++)
                {
                    PacketMessage packet = packets[i];
                    _ = Handlers.TryGetValue(packet.Id, out Action<IMessage> handler);
                    handler?.Invoke(packet.Message);
                }

                await UniTask.Yield();
            }
        }

        public async UniTaskVoid UpdateServerTime( CancellationTokenSource cts )
        {
            while (!cts.IsCancellationRequested && state == ConnectionState.NORMAL)
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();

                    serverTime += stopwatch.ElapsedMilliseconds;
                    calcuatedServerTime = serverTime;
                    delTime = 0;

                    stopwatch.Reset();
                }
                else
                {
                    delTime += Time.deltaTime;
                    calcuatedServerTime = serverTime + (long)Math.Round(delTime * 1000, 1);
                }

                await UniTask.Yield();
            }
        }
    }
}
