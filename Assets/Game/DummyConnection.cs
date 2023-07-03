using Cysharp.Threading.Tasks;
using Framework.Network;
using Protocol;
using System;

public class DummyConnection : Connection
{
    private int gameObjectId;
    private float p_x, p_y, p_z;
    private int r_x, r_y, r_z;

    public void Handle_S_ENTER( Protocol.S_ENTER enter )
    {
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

        r_x = UnityEngine.Random.Range(-45, 45);
        r_y = UnityEngine.Random.Range(-45, 45);
        r_z = UnityEngine.Random.Range(-45, 45);

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

    public void Handle_S_INSTANTIATE_GAME_OBJECT( Protocol.S_INSTANTIATE_GAME_OBJECT enter )
    {
        Protocol.C_LEAVE leave = new();
        Send(PacketManager.MakeSendBuffer(leave));

        //gameObjectId = enter.GameObjectId;
        //Rotate().Forget();
    }

    private async UniTask Rotate()
    {
        Protocol.C_SET_TRANSFORM st = new()
        {
            GameObjectId = gameObjectId
        };

        Protocol.Vector3 position = new();
        st.Position = position;
        position.X = p_x;
        position.Y = p_y;
        position.Z = p_z;

        Protocol.Vector3 rotation = new();
        st.Rotation = rotation;

        while (state == ConnectionState.NORMAL)
        {
            r_x = (r_x + UnityEngine.Random.Range(1, 5)) % 360;
            r_y = (r_y + UnityEngine.Random.Range(1, 5)) % 360;
            r_z = (r_z + UnityEngine.Random.Range(1, 5)) % 360;

            rotation.X = r_x;
            rotation.Y = r_y;
            rotation.Z = r_z;

            Send(PacketManager.MakeSendBuffer(st));

            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        }
    }
}