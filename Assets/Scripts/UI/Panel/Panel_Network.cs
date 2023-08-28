using Framework.Network;
using Protocol;
using TMPro;
using UnityEngine.UI;

public class Panel_Network : Panel_Base
{
    private TMP_InputField inputField_ClientId;
    private Button btn_CreateMain;
    private Button btn_DestroyMain;
    private TMP_InputField inputField_DummyNumber;
    private Button btn_AddDummy;
    private Button btn_RemoveDummy;
    private Button btn_AddBall;
    private Button btn_RemoveBall;

    protected override void Awake()
    {
        base.Awake();

        inputField_ClientId = transform.Search(nameof(inputField_ClientId)).GetComponent<TMP_InputField>();

        btn_CreateMain = GetUI_Button(nameof(btn_CreateMain), OnClick_Connect);
        btn_DestroyMain = GetUI_Button(nameof(btn_DestroyMain), OnClick_Disconnect);

        inputField_DummyNumber = transform.Search(nameof(inputField_DummyNumber)).GetComponent<TMP_InputField>();

        btn_AddDummy = GetUI_Button(nameof(btn_AddDummy), OnClick_AddDummy);
        btn_RemoveDummy = GetUI_Button(nameof(btn_RemoveDummy), OnClick_RemoveDummy);

        inputField_ClientId.placeholder.GetComponent<TMP_Text>().text = GenerateRandomString(5);

        btn_AddBall = GetUI_Button(nameof(btn_AddBall), OnClick_AddBall);
        btn_RemoveBall = GetUI_Button(nameof(btn_RemoveBall), OnClick_RemoveBall);
    }

    private void OnClick_Connect()
    {
        string clientId = inputField_ClientId.text;

        if (string.IsNullOrEmpty(clientId))
        {
            clientId = inputField_ClientId.placeholder.GetComponent<TMP_Text>().text;
        }

        GameClientManager.Instance.Connect(clientId);
    }

    private void OnClick_Disconnect()
    {
        GameClientManager.Instance.Disconnect();
    }

    private void OnClick_AddDummy()
    {
        string clientId = inputField_ClientId.text;

        if (string.IsNullOrEmpty(clientId))
        {
            clientId = inputField_ClientId.placeholder.GetComponent<TMP_Text>().text;
        }

        //string to int
        int dummyNumber = string.IsNullOrEmpty(inputField_DummyNumber.text)
            ? int.Parse(inputField_DummyNumber.placeholder.GetComponent<TMP_Text>().text)
            : int.Parse(inputField_DummyNumber.text);
        GameClientManager.Instance.AddDummy(dummyNumber, clientId);
    }

    private void OnClick_RemoveDummy()
    {
        GameClientManager.Instance.RemoveDummy();
    }

    private void OnClick_AddBall()
    {
        print("OnClick_AddBall");

        {
            C_INSTANTIATE_GAME_OBJECT packet = new();

            packet.Type = Define.GAMEOBJECT_TYPE_ETC;

            Protocol.Vector3 position = new()
            {
                X = 0f,
                Y = 2f,
                Z = 3f
            };
            packet.Position = position;

            Protocol.Vector3 rotation = new()
            {
                X = 0f,
                Y = 0f,
                Z = 0f
            };
            packet.Rotation = rotation;

            packet.PrefabName = "Ball";

            GameClientManager.Instance.Client.Send(PacketManager.MakeSendBuffer(packet));
        }
    }

    private void OnClick_RemoveBall()
    {
        print("OnClick_RemoveBall");
    }

    public string GenerateRandomString( int length )
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random random = new();
        char[] randomString = new char[length];

        for (int i = 0; i < length; i++)
        {
            randomString[i] = chars[random.Next(chars.Length)];
        }

        return new string(randomString);
    }
}
