using Google.Protobuf;
using System.Collections.Generic;

namespace Framework.Network
{
    public class PacketMessage
    {
        public ushort Id { get; set; }
        public IMessage Message { get; set; }
    }

    public class PacketQueue
    {
        private readonly Queue<PacketMessage> _packetQueue = new();
        private readonly object @lock = new();

        public void Push( ushort id, IMessage packet )
        {
            lock (@lock)
            {
                _packetQueue.Enqueue(new PacketMessage() { Id = id, Message = packet });
            }
        }

        public PacketMessage Pop()
        {
            lock (@lock)
            {
                return _packetQueue.Count == 0 ? null : _packetQueue.Dequeue();
            }
        }

        public List<PacketMessage> PopAll()
        {
            List<PacketMessage> list = new();

            lock (@lock)
            {
                while (_packetQueue.Count > 0)
                {
                    list.Add(_packetQueue.Dequeue());
                }
            }

            return list;
        }

        public bool Empty()
        {
            lock (@lock)
            {
                return _packetQueue.Count == 0;
            }
        }
    }
}