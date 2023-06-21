using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Framework.Network
{
    public class CustomSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        public Connection Connection { get; set; }
    }

    public static class ConnectionManager
    {
        private static readonly Dictionary<string, Connection> connections = new();

        private static int idGenerator = 0;

        public static Connection GetConnection<T>() where T : Connection, new()
        {
            Connection connection = new T();

            connection.ConnectionId = idGenerator++.ToString();
            connections.Add(connection.ConnectionId, connection);
            return connection;
        }

        public static void Connect( IPEndPoint endPoint, string connectionId )
        {
            if (!connections.TryGetValue(connectionId, out Connection connection)) { return; }

            Socket socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            CustomSocketAsyncEventArgs args = new();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;
            args.Connection = connection;

            RegisterConnect(args);
        }

        private static void RegisterConnect( CustomSocketAsyncEventArgs args )
        {
            if (args.UserToken is not Socket socket)
            {
                return;
            }

            bool pending = socket.ConnectAsync(args);
            if (pending == false)
            {
                OnConnectCompleted(null, args);
            }
        }

        private static void OnConnectCompleted( object sender, SocketAsyncEventArgs _args )
        {
            CustomSocketAsyncEventArgs args = _args as CustomSocketAsyncEventArgs;

            if (args.SocketError == System.Net.Sockets.SocketError.Success)
            {
                args.Connection.Session.Start(args.ConnectSocket);
                args.Connection.Session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                //Connection Fail Callback
            }
        }
    }
}