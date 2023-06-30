using Framework.Network;
using Protocol;

public class MainConnection : Connection
{
    public void Handle_S_ENTER( Protocol.S_ENTER enter )
    {
        {
            C_GET_GAME_OBJECT packet = new();
            Send(PacketManager.MakeSendBuffer(packet));
        }

        {
            C_INSTANTIATE_GAME_OBJECT packet = new();

            Protocol.Vector3 position = new()
            {
                X = 0f,
                Y = 0f,
                Z = 0f
            };

            Protocol.Vector3 rotation = new()
            {
                X = 0f,
                Y = 0f,
                Z = 0f
            };

            packet.Position = position;
            packet.Rotation = rotation;

            Send(PacketManager.MakeSendBuffer(packet));
        }

        //{
        //    Protocol.C_TEST packet = new();
        //    Send(PacketManager.MakeSendBuffer(packet));
        //}
    }
}