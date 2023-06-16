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

    [SerializeField] List<Connection> connections = new List<Connection>();

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

        btn_Connect = GameObject.Find(nameof(btn_Connect)).GetComponent<Button>();
        btn_CreateDummy = GameObject.Find(nameof(btn_CreateDummy)).GetComponent<Button>();
        btn_DestroyDummy = GameObject.Find(nameof(btn_DestroyDummy)).GetComponent<Button>();

        btn_Connect.onClick.AddListener(ConnectToServer);
        btn_CreateDummy.onClick.AddListener(CreateDummy);
        btn_DestroyDummy.onClick.AddListener(DestroyDummy);

        inputField_IpAddress.placeholder.GetComponent<TMP_Text>().text = "192.168.0.104";
        inputField_Port.placeholder.GetComponent<TMP_Text>().text = "7777";
    }

	public void ConnectToServer()
    {
        Debug.Log("Connect To Server");

        var ip = inputField_IpAddress.text == string.Empty ? "192.168.0.104" : inputField_IpAddress.text;
        var port = inputField_Port.text == string.Empty ? 7777 : int.Parse(inputField_Port.text);

        var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);

        var connection = new Connection();
        connections.Add(connection);

        var connector = new Connector();
        connector.Connect(endPoint, () => connection.session);

        Debug.Log("ip : " + ip + ", port : " + port);
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
