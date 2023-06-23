using Framework.Network;
using Protocol;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientManager : MonoBehaviour
{
    private TMP_InputField inputField_IpAddress;
    private TMP_InputField inputField_Port;
    [SerializeField] private TMP_InputField inputField_ConnectionId;
    private Button btn_Connect;
    private Button btn_CreateMain;
    private Button btn_DestroyMain;
    private Button btn_CreateDummy;
    private Button btn_DestroyDummy;
    private GameObject go_Main;
    private GameObject go_Dummy;
    private readonly Dictionary<string, GameObject> gameObjects = new();
    public GameObject dummyPrefab;
    public int dummyCount = 0;

    private void OnDestroy()
    {
        btn_Connect.onClick.RemoveAllListeners();
        btn_CreateDummy.onClick.RemoveAllListeners();
        btn_DestroyDummy.onClick.RemoveAllListeners();
    }

    private void Awake()
    {
        inputField_IpAddress = GameObject.Find(nameof(inputField_IpAddress)).GetComponent<TMP_InputField>();
        inputField_Port = GameObject.Find(nameof(inputField_Port)).GetComponent<TMP_InputField>();
        inputField_ConnectionId = GameObject.Find(nameof(inputField_ConnectionId)).GetComponent<TMP_InputField>();

        btn_Connect = GameObject.Find(nameof(btn_Connect)).GetComponent<Button>();

        btn_CreateDummy = GameObject.Find(nameof(btn_CreateDummy)).GetComponent<Button>();
        btn_DestroyDummy = GameObject.Find(nameof(btn_DestroyDummy)).GetComponent<Button>();
        btn_CreateMain = GameObject.Find(nameof(btn_CreateMain)).GetComponent<Button>();
        btn_DestroyMain = GameObject.Find(nameof(btn_DestroyMain)).GetComponent<Button>();

        //btn_Connect.onClick.AddListener(() => ConnectToServer("192.168.0.104", 7777));

        btn_CreateDummy.onClick.AddListener(OnClick_CreateDummy);
        btn_DestroyDummy.onClick.AddListener(OnClick_DestroyDummy);
        btn_CreateMain.onClick.AddListener(OnClick_CreateMain);
        btn_DestroyMain.onClick.AddListener(OnClick_DestroyMain);

        inputField_IpAddress.placeholder.GetComponent<TMP_Text>().text = "192.168.0.104";
        inputField_Port.placeholder.GetComponent<TMP_Text>().text = "7777";

        go_Main = GameObject.Find("Main");
        go_Dummy = GameObject.Find("Dummy");
    }

    private readonly Dictionary<string, Connection> dummyConnections = new();
    static int idGenerator = 0;

    public async void OnClick_CreateDummy()
    {
        IPEndPoint endPoint = new(IPAddress.Parse("192.168.0.104"), 7777);

        for(int i = 0; i<200; i++)
        {
            DummyConnection connection = (DummyConnection)ConnectionManager.GetConnection<DummyConnection>();

            connection.AddHandler(connection.Handle_S_ENTER);
            connection.AddHandler(connection.Handle_S_INSTANTIATE_GAME_OBJECT);

            dummyConnections.Add(connection.ConnectionId, connection);
            bool success = await ConnectionManager.Connect(endPoint, connection);
            if (success)
            {
                Protocol.C_ENTER enter = new();
                enter.ClientId = "Dummy_" + idGenerator++.ToString();
                connection.Send(PacketManager.MakeSendBuffer(enter));
            }
            else
            {
                UnityEngine.Debug.Log("Enter Fail!");
            }
        }
    }

    public async void OnClick_DestroyDummy()
    {
        foreach (KeyValuePair<string, Connection> dummyConnection in dummyConnections)
        {
            dummyConnection.Value.Send(PacketManager.MakeSendBuffer(new Protocol.C_LEAVE()));

            await Task.Delay(10);
        }
        dummyConnections.Clear();
    }

    private Connection mainConnection = null;

    public async void OnClick_CreateMain()
    {
        if (mainConnection != null)
        {
            return;
        }

        IPEndPoint endPoint = new(IPAddress.Parse("192.168.0.104"), 7777);
        MainConnection connection = (MainConnection)ConnectionManager.GetConnection<MainConnection>();

        mainConnection = connection;

        mainConnection.AddHandler(connection.Handle_S_ENTER);
        mainConnection.AddHandler(AddGameObject);
        mainConnection.AddHandler(RemoveGameObject);
        mainConnection.AddHandler(SetTransform);

        bool success = await ConnectionManager.Connect(endPoint, connection);
        if (success)
        {
            Protocol.C_ENTER enter = new();
            enter.ClientId = "Main";
            connection.Send(PacketManager.MakeSendBuffer(enter));
        }
    }

    public void OnClick_DestroyMain()
    {
        mainConnection.Send(PacketManager.MakeSendBuffer(new Protocol.C_LEAVE()));
        mainConnection = null;
    }

    public void AddGameObject( Protocol.S_ADD_GAME_OBJECT pkt )
    {
        foreach (S_ADD_GAME_OBJECT.Types.GameObjectInfo gameObject in pkt.GameObjects)
        {
            UnityEngine.Vector3 position = new(gameObject.Position.X, gameObject.Position.Y, gameObject.Position.Z);
            Quaternion rotation = Quaternion.Euler(gameObject.Rotation.X, gameObject.Rotation.Y, gameObject.Rotation.Z);

            GameObject dummy = Instantiate(dummyPrefab, UnityEngine.Vector3.zero, Quaternion.identity, go_Dummy.transform);
            dummy.transform.localPosition = position;
            dummy.transform.localRotation = rotation;
            dummy.name = gameObject.Id.ToString();

            if (!gameObjects.ContainsKey(dummy.name))
            {
                gameObjects.Add(dummy.name, dummy);
            }
        }
    }

    public void RemoveGameObject( Protocol.S_REMOVE_GAME_OBJECT pkt )
    {
        foreach (int gameObjectId in pkt.GameObjects)
        {
            Destroy(gameObjects[gameObjectId.ToString()]);
            _ = gameObjects.Remove(gameObjectId.ToString());
        }
    }

    public void SetTransform( Protocol.S_SET_TRANSFORM pkt )
    {
        GameObject go = gameObjects[pkt.GameObjectId.ToString()];

        if(go == null)
        {
            return;
        }

        UnityEngine.Vector3 position = new(pkt.Position.X, pkt.Position.Y, pkt.Position.Z);
        Quaternion rotation = Quaternion.Euler(pkt.Rotation.X, pkt.Rotation.Y, pkt.Rotation.Z);

        go.transform.localPosition = position;
        go.transform.localRotation = rotation;
    }
}
