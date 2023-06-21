using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Framework.Network;
using System.Net;
using System.Collections.Generic;
using Protocol;
using UnityEditor.Experimental.GraphView;

public class ClientManager : MonoBehaviour
{
    TMP_InputField inputField_IpAddress;
    TMP_InputField inputField_Port;
    [SerializeField] TMP_InputField inputField_ConnectionId;

    Button btn_Connect;

    Button btn_CreateMain;
    Button btn_DestroyMain;

    Button btn_CreateDummy;
    Button btn_DestroyDummy;

    GameObject go_Main;
    GameObject go_Dummy;

    Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
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

    Dictionary<string, Connection> dummyConnections = new();

    public void OnClick_CreateDummy()
	{
        var endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.104"), 7777);
        var connection = ConnectionManager.GetConnection<DummyConnection>();
        dummyConnections.Add(connection.ConnectionId, connection);
        ConnectionManager.Connect(endPoint, connection.ConnectionId);
    }

    public void OnClick_DestroyDummy()
	{
  //      foreach(var item in connections)
		//{
  //          item.Value.Send(new C_LEAVE());
  //      }
    }

    Connection mainConnection = null;

    public void OnClick_CreateMain()
    {
        if (mainConnection != null)
            return;

        var endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.104"), 7777);
        var connection = ConnectionManager.GetConnection<MainConnection>();

        connection.clientId = inputField_ConnectionId.text;

        mainConnection = connection;

        mainConnection.AddHandler(AddGameObject);
        mainConnection.AddHandler(RemoveGameObject);
        mainConnection.AddHandler(SetTransform);

        ConnectionManager.Connect(endPoint, connection.ConnectionId);
    }

    public void OnClick_DestroyMain()
    {
        Debug.Log("OnClick_DestroyMain");
    }

    public void AddGameObject(Protocol.S_ADD_GAME_OBJECT pkt)
	{
        foreach(var gameObject in pkt.GameObjects)
        {
            var position = new UnityEngine.Vector3(gameObject.Position.X, gameObject.Position.Y, gameObject.Position.Z);
            var rotation = Quaternion.Euler(gameObject.Rotation.X, gameObject.Rotation.Y, gameObject.Rotation.Z);

            var dummy = Instantiate(dummyPrefab, UnityEngine.Vector3.zero, Quaternion.identity, go_Dummy.transform);
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
        foreach (var gameObjectId in pkt.GameObjects)
        {
            Destroy(gameObjects[gameObjectId.ToString()]);
            gameObjects.Remove(gameObjectId.ToString());
        }
    }

    public void SetTransform( Protocol.S_SET_TRANSFORM pkt )
    {
        var go = gameObjects[pkt.GameObjectId.ToString()];

        var position = new UnityEngine.Vector3(pkt.Position.X, pkt.Position.Y, pkt.Position.Z);
        var rotation = Quaternion.Euler(pkt.Rotation.X, pkt.Rotation.Y, pkt.Rotation.Z);

        go.transform.localPosition = position;
        go.transform.localRotation = rotation;
    }
}
