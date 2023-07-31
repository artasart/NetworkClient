using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class Panel_Network : Panel_Base
{
	TMP_InputField inputField_ConnectionId;

	Button btn_Connect;
	Button btn_CreateMain;
	Button btn_DestroyMain;

	protected override void Awake()
	{
		base.Awake();

        inputField_ConnectionId = this.transform.Search(nameof(inputField_ConnectionId)).GetComponent<TMP_InputField>();

        btn_Connect = GetUI_Button(nameof(btn_Connect));

        btn_CreateMain = GetUI_Button(nameof(btn_CreateMain), OnClick_CreateMain);
        btn_DestroyMain = GetUI_Button(nameof(btn_DestroyMain), OnClick_DestroyMain);

		inputField_ConnectionId.placeholder.GetComponent<TMP_Text>().text = GenerateRandomString(5);
    }

	private void OnClick_CreateMain()
	{
		var virtualCamrea = GameObject.Find("1").GetComponent<CinemachineVirtualCamera>();

		CinemachineSwitcher.SwitchCamera(virtualCamrea);

		var connectionId = inputField_ConnectionId.text;
        if(string.IsNullOrEmpty(connectionId))
        {
            connectionId = inputField_ConnectionId.placeholder.GetComponent<TMP_Text>().text;
        }

		GameClientManager.Instance.CreateMain(connectionId);
	}

	private void OnClick_DestroyMain()
	{
        CinemachineSwitcher.SwitchMainCamera(CinemachineBlendDefinition.Style.EaseInOut);

        GameClientManager.Instance.DestroyMain();
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
