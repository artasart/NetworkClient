using Cysharp.Threading.Tasks;
using Framework.Network;
using Protocol;
using System;
using UnityEngine;

public class DummyConnection : Connection
{
    private static int idGenerator = 0;

    private readonly string clientId;

    int gameObjectId;
    float p_x, p_y, p_z;
    float r_x, r_y, r_z;

    public DummyConnection()
    {
        clientId = "Dummy_" + idGenerator++.ToString();

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

        p_x = UnityEngine.Random.Range(-3f, 3f);
        p_y = UnityEngine.Random.Range(-3f, 3f);
        p_z = UnityEngine.Random.Range(-3f, 3f);

        Protocol.Vector3 position = new()
        {
            X = p_x,
            Y = p_y,
            Z = p_z
        };

        r_x = UnityEngine.Random.Range(-45f, 45f);
        r_y = UnityEngine.Random.Range(-45f, 45f);
        r_z = UnityEngine.Random.Range(-45f, 45f);

        Protocol.Vector3 rotation = new()
        {
            X = r_x,
            Y = r_y,
            Z = r_z
        };

        packet.Position = position;
        packet.Rotation = rotation;

        Send(PacketManager.MakeSendBuffer(packet));
    }
}