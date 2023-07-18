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
        private Socket socket;
        private readonly object @lock = new();
        private readonly RecvBuffer recvBuffer = new(65535);
        private readonly Queue<ArraySegment<byte>> sendQueue = new();
        private readonly List<ArraySegment<byte>> pendingList = new();
        private readonly SocketAsyncEventArgs sendArgs = new();
        private readonly SocketAsyncEventArgs recvArgs = new();
        private bool isConnected = false;
        private bool isSendRegistered = false;
        private bool isDisconnectRegistered = false;

        public abstract void OnConnected( EndPoint endPoint );
        public abstract void OnDisconnected( EndPoint endPoint );
        public abstract int OnRecv( ArraySegment<byte> buffer );
        public abstract void OnSend( int numOfBytes );

        ~Session()
        {
            if (socket != null)
                socket.Close();
        }

        public void Start( Socket socket )
        {
            lock (@lock)
            {
                isConnected = true;
            }

            this.socket = socket;

            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void RegisterDisconnect()
        {
            lock (@lock)
            {
                if(isDisconnectRegistered)
                {
                    return;
                }

                isDisconnectRegistered = true;

                if (!isSendRegistered)
                {
                    Disconnect();
                }
            }
        }

        public void Disconnect()
        {
            lock (@lock)
            {
                if(!isConnected)
                {
                    return;
                }

                isConnected = false;
            }

            try
            {
                OnDisconnected(socket.RemoteEndPoint);
            }
            catch (Exception e)
            {
                Debug.Log($"Get RemoteEndPoint Failed {e}");
            }

            try
            {
                socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception e)
            {
                Debug.Log($"Shutdown Failed {e}");
            }

            sendQueue.Clear();
            pendingList.Clear();
        }

        public void Send( ArraySegment<byte> sendBuff )
        {
            bool registerSend = false;

            lock (@lock)
            {
                if (!isConnected || isDisconnectRegistered)
                {
                    return;
                }

                sendQueue.Enqueue(sendBuff);

                if (!isSendRegistered)
                {
                    isSendRegistered = true;
                    registerSend = true;
                }
            }

            if(registerSend)
            {
                RegisterSend();
            }
        }

        private void RegisterSend()
        {
            lock (@lock)
            {
                while (sendQueue.Count > 0)
                {
                    pendingList.Add(sendQueue.Dequeue());
                }
            }

            sendArgs.BufferList = pendingList;

            try
            {
                bool pending = socket.SendAsync(sendArgs);
                if (pending == false)
                {
                    OnSendCompleted(null, sendArgs);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"RegisterSend Failed {e}");
            }
        }

        private void OnSendCompleted( object sender, SocketAsyncEventArgs args )
        {
            if (args.BytesTransferred > 0 && args.SocketError == System.Net.Sockets.SocketError.Success)
            {
                try
                {
                    lock (@lock)
                    {
                        sendArgs.BufferList = null;
                        pendingList.Clear();

                        OnSend(sendArgs.BytesTransferred);

                        if (isDisconnectRegistered)
                        {
                            //isSendRegistered = false;
                            Disconnect();
                            return;
                        }

                        if (sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                        else
                        {
                            isSendRegistered = false;
                        }
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

        private void RegisterRecv()
        {
            recvBuffer.Clean();
            ArraySegment<byte> segment = recvBuffer.WriteSegment;
            recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = socket.ReceiveAsync(recvArgs);
                if (pending == false)
                {
                    OnRecvCompleted(null, recvArgs);
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
                    if (recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Debug.Log($"OnWrite Fail, Disconnect");
                        RegisterDisconnect();
                        return;
                    }

                    int processLen = OnRecv(recvBuffer.ReadSegment);
                    if (processLen < 0 || recvBuffer.DataSize < processLen)
                    {
                        Debug.Log($"OnRecv Fail, Disconnect");
                        RegisterDisconnect();
                        return;
                    }

                    if (recvBuffer.OnRead(processLen) == false)
                    {
                        Debug.Log($"OnRead Fail, Disconnect");
                        RegisterDisconnect();
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
                RegisterDisconnect();
            }
        }
    }
}
