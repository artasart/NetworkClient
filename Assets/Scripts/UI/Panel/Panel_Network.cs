using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class Panel_Network : Panel_Base
{
	TMP_InputField inputField_ClientId;

	Button btn_CreateMain;
	Button btn_DestroyMain;

    public bool isConnect = false;

	protected override void Awake()
	{
		base.Awake();

        inputField_ClientId = this.transform.Search(nameof(inputField_ClientId)).GetComponent<TMP_InputField>();


        btn_CreateMain = GetUI_Button(nameof(btn_CreateMain), OnClick_Connect);
        btn_DestroyMain = GetUI_Button(nameof(btn_DestroyMain), OnClick_Disconnect);

		inputField_ClientId.placeholder.GetComponent<TMP_Text>().text = GenerateRandomString(5);
    }

	private void OnClick_Connect()
	{
        if (isConnect) return;

		var clientId = inputField_ClientId.text;

        if(string.IsNullOrEmpty(clientId))
        {
            clientId = inputField_ClientId.placeholder.GetComponent<TMP_Text>().text;
        }

		GameClientManager.Instance.Connect(clientId);

        isConnect = true;
    }

	private void OnClick_Disconnect()
	{
        GameClientManager.Instance.Disconnect();
    }

    public string GenerateRandomString( int length )
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        var randomString = new char[length];

        for (int i = 0; i < length; i++)
        {
            randomString[i] = chars[random.Next(chars.Length)];
        }

        return new string(randomString);
    }
}
