using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

namespace FrameWork.Network
{
    public class CustomSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        public string ConnectionId { get; set; }
    }

    public class Connector
    {
        Dictionary<string, Connection> connections;

        public Connector( Dictionary<string, Connection> _connections)
        {
            connections = _connections;
        }

        public void Connect(IPEndPoint endPoint, string connectionId)
        {
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            CustomSocketAsyncEventArgs args = new CustomSocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;
            args.ConnectionId = connectionId;

            RegisterConnect(args);
        }

        void RegisterConnect( CustomSocketAsyncEventArgs args )
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
            {
                return;
            }

            bool pending = socket.ConnectAsync(args);
            if (pending == false)
                OnConnectCompleted(null, args);
        }

        void OnConnectCompleted(object sender, SocketAsyncEventArgs _args)
        {
            CustomSocketAsyncEventArgs args = _args as CustomSocketAsyncEventArgs;

            if (args.SocketError == System.Net.Sockets.SocketError.Success
                && connections[args.ConnectionId] != null)
            {
                Session session = connections[args.ConnectionId].session;
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
        }
    }
}