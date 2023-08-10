using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.InputSystem.Controls;
using System;

public class Panel_Network : Panel_Base
{
    TMP_InputField inputField_ClientId;

    Button btn_CreateMain;
	Button btn_DestroyMain;

    TMP_InputField inputField_DummyNumber;

    Button btn_AddDummy;
    Button btn_RemoveDummy;

    protected override void Awake()
	{
		base.Awake();

        inputField_ClientId = this.transform.Search(nameof(inputField_ClientId)).GetComponent<TMP_InputField>();

        btn_CreateMain = GetUI_Button(nameof(btn_CreateMain), OnClick_Connect);
        btn_DestroyMain = GetUI_Button(nameof(btn_DestroyMain), OnClick_Disconnect);

        inputField_DummyNumber = this.transform.Search(nameof(inputField_DummyNumber)).GetComponent<TMP_InputField>();

        btn_AddDummy = GetUI_Button(nameof(btn_AddDummy), OnClick_AddDummy);
        btn_RemoveDummy = GetUI_Button(nameof(btn_RemoveDummy), OnClick_RemoveDummy);

		inputField_ClientId.placeholder.GetComponent<TMP_Text>().text = GenerateRandomString(5);
    }

	private void OnClick_Connect()
	{
		var clientId = inputField_ClientId.text;

        if(string.IsNullOrEmpty(clientId))
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
        var clientId = inputField_ClientId.text;

        if (string.IsNullOrEmpty(clientId))
        {
            clientId = inputField_ClientId.placeholder.GetComponent<TMP_Text>().text;
        }

        //string to int
        int dummyNumber;

        if (string.IsNullOrEmpty(inputField_DummyNumber.text))
        {
            dummyNumber = Int32.Parse(inputField_DummyNumber.placeholder.GetComponent<TMP_Text>().text);
        }
        else
        {
            dummyNumber = Int32.Parse(inputField_DummyNumber.text);
        }

        GameClientManager.Instance.AddDummy(dummyNumber, clientId);
    }

    private void OnClick_RemoveDummy()
    {
        GameClientManager.Instance.RemoveDummy();
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
