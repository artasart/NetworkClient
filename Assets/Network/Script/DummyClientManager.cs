using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FrameWork.Network;
using System.Net;
using System.Collections.Generic;

public class DummyClientManager : MonoBehaviour
{
    // 192.168.0.104
    // 7777

    TMP_InputField inputField_IpAddress;
    TMP_InputField inputField_Port;

    Button btn_Connect;

    Button btn_CreateDummy;
    Button btn_DestroyDummy;

    RealtimePacket realtimePacket;

    //[SerializeField] List<Connection> connections = new List<Connection>();

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

        btn_Connect.onClick.AddListener(() => ConnectToServer("192.168.0.104", 7777));
        btn_CreateDummy.onClick.AddListener(CreateDummy);
        btn_DestroyDummy.onClick.AddListener(DestroyDummy);

        inputField_IpAddress.placeholder.GetComponent<TMP_Text>().text = "192.168.0.104";
        inputField_Port.placeholder.GetComponent<TMP_Text>().text = "7777";
    }

    private int idGenerator = 0;
    private Dictionary<string, Connection> connections;
    private Connector connector;

    private void Start()
    {
        idGenerator = 0;
        connections =  new Dictionary<string, Connection>();
        connector = new(() => {       
            var connection = new Connection(idGenerator++.ToString(), realtimePacket);
            connections[connection.ConnectionId] = connection;
            return connection.session;
        });
    }

	public void ConnectToServer(string ip, int port)
    {
        //var ip = inputField_IpAddress.text == string.Empty ? "192.168.0.104" : inputField_IpAddress.text;
        //var port = inputField_Port.text == string.Empty ? 7777 : int.Parse(inputField_Port.text);
        var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        connector.Connect(endPoint);
    }

    public void CreateDummy()
	{
        Debug.Log("Dummy Client Created!");
    }

    public void DestroyDummy()
	{
        Debug.Log("Dummy Client Destroyed!");
    }
}
