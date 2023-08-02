using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Framework.Network
{
    public class CustomSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        public Connection Connection { get; set; }
    }

    public static class ConnectionManager
    {
        private static int idGenerator = 0;
        private static readonly Dictionary<string, Connection> connections = new();

        public static Connection GetConnection<T>() where T : Connection, new()
        {
            Connection connection = new T
            {
                ConnectionId = idGenerator++.ToString()
            };
            connections[connection.ConnectionId] = connection;
            return connection;
        }

        public static Connection GetConnection( string connectionId )
        {
            return connections[connectionId];
        }

        public static async Task<bool> Connect( IPEndPoint endPoint, Connection connection )
        {
            try
            {
                Socket socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(endPoint);

                ServerSession session = new();
                connection.Session = session;
                connection.Session.Start(socket);
                connection.Session.OnConnected(endPoint);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return false;
            }
        }
    }
}