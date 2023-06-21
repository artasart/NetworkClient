using Google.Protobuf;
using System;
using Cysharp.Threading.Tasks;

namespace Framework.Network
{
	public class Connection : PacketHandler
	{
		public string ConnectionId { get; set; }

        public ServerSession Session { get; set; }
        protected bool isConnected;
        public Action connectedHandler;
        public Action disconnectedHandler;

        private PacketQueue packetQueue;
        private DateTime lastMessageSent;


        public Connection()
		{
            isConnected = false;
            
            Session = new();

            Session.connectedHandler += OnConnected;
            Session.disconnectedHandler += OnDisConnected;
            Session.receivedHandler += OnRecv;

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

            disconnectedHandler?.Invoke();
        }

		private void OnRecv(ArraySegment<byte> buffer)
		{
            PacketManager.OnRecv(buffer, packetQueue);
        }

        public void Send(ArraySegment<byte> pkt)
        {
            lastMessageSent = DateTime.UtcNow;
            Session.Send(pkt);
        }

		public async UniTaskVoid AsyncPacketUpdate()
		{
            Action<IMessage> handler;

            while (isConnected)
			{
                var packets = packetQueue.PopAll();

                for (var i = 0; i < packets.Count; i++)
                {
					var packet = packets[i];
					Handlers.TryGetValue(packet.Id, out handler);
                    handler?.Invoke(packet.Message);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.02f));
			}

            disconnectedHandler?.Invoke();
		}

        public async UniTaskVoid HeartBeat()
        {
            Protocol.C_HEARTBEAT heartbeat = new();
            var heartbeatPkt = PacketManager.MakeSendBuffer(heartbeat);

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
