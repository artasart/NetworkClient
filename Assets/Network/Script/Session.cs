using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Framework.Network
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        public sealed override int OnRecv( ArraySegment<byte> buffer )
        {
            int processLen = 0;

            while (true)
            {
                if (buffer.Count < HeaderSize)
                {
                    break;
                }

                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);

                if (buffer.Count < dataSize)
                {
                    break;
                }

                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLen;
        }

        public abstract void OnRecvPacket( ArraySegment<byte> buffer );
    }

    public abstract class Session
    {
        private Socket _socket;
        private int _disconnected = 0;
        private readonly RecvBuffer _recvBuffer = new(65535);
        private readonly object _lock = new();
        private readonly Queue<ArraySegment<byte>> _sendQueue = new();
        private readonly List<ArraySegment<byte>> _pendingList = new();
        private readonly SocketAsyncEventArgs _sendArgs = new();
        private readonly SocketAsyncEventArgs _recvArgs = new();

        bool isSendRegistered = false;
        bool isDisconnectRegistered = false;

        public abstract void OnConnected( EndPoint endPoint );
        public abstract int OnRecv( ArraySegment<byte> buffer );
        public abstract void OnSend( int numOfBytes );
        public abstract void OnDisconnected( EndPoint endPoint );

        private void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Start( Socket socket )
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send( List<ArraySegment<byte>> sendBuffList )
        {
            if (sendBuffList.Count == 0)
            {
                return;
            }

            lock (_lock)
            {
                if(isDisconnectRegistered)
                {
                    return;
                }

                foreach (ArraySegment<byte> sendBuff in sendBuffList)
                {
                    _sendQueue.Enqueue(sendBuff);
                }

                if (_pendingList.Count == 0)
                {
                    isSendRegistered = true;
                    RegisterSend();
                }
            }
        }

        public void Send( ArraySegment<byte> sendBuff )
        {
            lock (_lock)
            {
                if (isDisconnectRegistered)
                {
                    return;
                }

                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                {
                    isSendRegistered = true;
                    RegisterSend();
                }
            }
        }

        public void RegisterDisconnect()
        {
            lock(_lock)
            {
                isDisconnectRegistered = true;

                if(isSendRegistered == false)
                {
                    Disconnect();
                }
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            {
                return;
            }

            try
            {
                OnDisconnected(_socket.RemoteEndPoint);
                _socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception e)
            {
                Debug.Log($"Shutdown Failed {e}");
            }
            
            _socket.Close();
            Clear();
        }

        private void RegisterSend()
        {
            if (_disconnected == 1)
            {
                return;
            }

            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }
            _sendArgs.BufferList = _pendingList;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                {
                    OnSendCompleted(null, _sendArgs);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"RegisterSend Failed {e}");
            }
        }

        private void OnSendCompleted( object sender, SocketAsyncEventArgs args )
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == System.Net.Sockets.SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if(isDisconnectRegistered)
                        {
                            Disconnect();
                            return;
                        }

                        if (_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Debug.Log($"OnSendCompleted Fail, Disconnect {args.SocketError}");
                    Disconnect();
                }
            }
        }

        private void RegisterRecv()
        {
            if (_disconnected == 1)
            {
                return;
            }

            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                {
                    OnRecvCompleted(null, _recvArgs);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"RegisterRecv Failed {e}");
            }
        }

        private void OnRecvCompleted( object sender, SocketAsyncEventArgs args )
        {
            if (args.SocketError == System.Net.Sockets.SocketError.Success && args.BytesTransferred > 0)
            {
                try
                {
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Debug.Log($"OnWrite Fail, Disconnect");
                        Disconnect();
                        return;
                    }

                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Debug.Log($"OnRecv Fail, Disconnect");
                        Disconnect();
                        return;
                    }

                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Debug.Log($"OnRead Fail, Disconnect");
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Debug.Log($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                Debug.Log($"OnRecvCompleted Fail, Disconnect {args.SocketError} {args.BytesTransferred}");
                Disconnect();
            }
        }
    }
}
