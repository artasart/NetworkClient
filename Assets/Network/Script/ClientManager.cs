using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FrameWork.Network;
using System.Net;
using System.Collections.Generic;
using Protocol;

public class ClientManager : MonoBehaviour
{
    #region Singleton

    public static ClientManager Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindObjectOfType<ClientManager>();
            return instance;
        }
    }
    private static ClientManager instance;

    #endregion

    TMP_InputField inputField_IpAddress;
    TMP_InputField inputField_Port;

    Button btn_Connect;

    Button btn_CreateMain;
    Button btn_DestroyMain;

    Button btn_CreateDummy;
    Button btn_DestroyDummy;

    RealtimePacket realtimePacket;

    public GameObject go_Main;
    public GameObject go_Dummy;

    private int idGenerator = 0;
    public Dictionary<string, Connection> connections;
	private Connector connector;

	Dictionary<string, GameObject> dummys = new Dictionary<string, GameObject>();
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
        realtimePacket = new RealtimePacket();

        inputField_IpAddress = GameObject.Find(nameof(inputField_IpAddress)).GetComponent<TMP_InputField>();
        inputField_Port = GameObject.Find(nameof(inputField_Port)).GetComponent<TMP_InputField>();

        btn_Connect = GameObject.Find(nameof(btn_Connect)).GetComponent<Button>();

        btn_CreateDummy = GameObject.Find(nameof(btn_CreateDummy)).GetComponent<Button>();
        btn_DestroyDummy = GameObject.Find(nameof(btn_DestroyDummy)).GetComponent<Button>();
        btn_CreateMain = GameObject.Find(nameof(btn_CreateMain)).GetComponent<Button>();
        btn_DestroyMain = GameObject.Find(nameof(btn_DestroyMain)).GetComponent<Button>();

        btn_Connect.onClick.AddListener(() => ConnectToServer("192.168.0.104", 7777));

        btn_CreateDummy.onClick.AddListener(OnClick_CreateDummy);
        btn_DestroyDummy.onClick.AddListener(OnClick_DestroyDummy);
        btn_CreateMain.onClick.AddListener(OnClick_CreateMain);
        btn_DestroyMain.onClick.AddListener(OnClick_DestroyMain);

        inputField_IpAddress.placeholder.GetComponent<TMP_Text>().text = "192.168.0.104";
        inputField_Port.placeholder.GetComponent<TMP_Text>().text = "7777";

        go_Main = GameObject.Find("Main");
        go_Dummy = GameObject.Find("Dummy");
    }

    private void Start()
    {
        idGenerator = 0;
        connections =  new Dictionary<string, Connection>();

        connector = new(() => {       
            //dummy connection
            var connection = new Connection(idGenerator++.ToString(), realtimePacket);

            connections[connection.ClientId] = connection;
                       
            return connection.session;
        });
    }

	public void ConnectToServer(string ip, int port)
    {
        var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);

        connector.Connect(endPoint);
    }

    public void OnClick_CreateDummy()
	{
        ConnectToServer("192.168.0.104", 7777);


    }

    public void OnClick_DestroyDummy()
	{
        foreach(var item in connections)
		{
            item.Value.Send(new C_LEAVE());
        }
    }

    public void OnClick_CreateMain()
    {
        Debug.Log("OnClick_CreateMain");
    }

    public void OnClick_DestroyMain()
    {
        Debug.Log("OnClick_DestroyMain");
    }

    public void CreateDummy(string _connectionId)
	{
        var dummy = Instantiate(dummyPrefab, UnityEngine.Vector3.zero, Quaternion.identity, go_Dummy.transform);

        dummy.transform.localPosition = new UnityEngine.Vector3(
            UnityEngine.Random.Range(-3f, 3f),
            UnityEngine.Random.Range(-3f, 3f),
            UnityEngine.Random.Range(-3f, 3f)
        );

        dummy.transform.localEulerAngles = new UnityEngine.Vector3(
            UnityEngine.Random.Range(-45f, 45f),
            UnityEngine.Random.Range(-45f, 45f),
            UnityEngine.Random.Range(-45f, 45f)
        );

        dummy.name = "Dummy_" + _connectionId;

        if (!dummys.ContainsKey(_connectionId))
		{
            dummys.Add(_connectionId, dummy);
        }
    }

    public void DestroyDummy(string _connectionId)
	{
        Destroy(dummys[_connectionId]);

        dummys.Remove(_connectionId);
    }
}
