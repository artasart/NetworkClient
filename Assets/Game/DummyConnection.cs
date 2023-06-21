using Framework.Network;
using Protocol;
using UnityEngine;

public class DummyConnection : Connection
{
    private static int idGenerator = 0;

    public DummyConnection()
    {
        AddHandler(Handle_S_ENTER);

        connectedHandler += () =>
        {
            Protocol.C_ENTER enter = new()
            {
                ClientId = clientId
            };
            Send(PacketManager.MakeSendBuffer(enter));
        };
    }

    private void Handle_S_ENTER( Protocol.S_ENTER enter )
    {
        Debug.Log("ENTER : " + enter.Result);

        C_INSTANTIATE_GAME_OBJECT packet = new();

        Protocol.Vector3 position = new()
        {
            X = UnityEngine.Random.Range(-3f, 3f),
            Y = UnityEngine.Random.Range(-3f, 3f),
            Z = UnityEngine.Random.Range(-3f, 3f)
        };

        Protocol.Vector3 rotation = new()
        {
            X = UnityEngine.Random.Range(-45f, 45f),
            Y = UnityEngine.Random.Range(-45f, 45f),
            Z = UnityEngine.Random.Range(-45f, 45f)
        };

        packet.Position = position;
        packet.Rotation = rotation;

        Send(PacketManager.MakeSendBuffer(packet));
    }
}