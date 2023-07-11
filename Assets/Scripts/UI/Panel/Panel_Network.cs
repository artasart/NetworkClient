using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class Panel_Network : Panel_Base
{
	TMP_InputField inputField_IpAddress;
	TMP_InputField inputField_Port;
	TMP_InputField inputField_ConnectionId;

	Button btn_Connect;
	Button btn_CreateMain;
	Button btn_DestroyMain;
	Button btn_CreateDummy;
	Button btn_DestroyDummy;

	protected override void Awake()
	{
		base.Awake();

		inputField_IpAddress = this.transform.Search(nameof(inputField_IpAddress)).GetComponent<TMP_InputField>();
        inputField_Port = this.transform.Search(nameof(inputField_Port)).GetComponent<TMP_InputField>();
        inputField_ConnectionId = this.transform.Search(nameof(inputField_ConnectionId)).GetComponent<TMP_InputField>();

        btn_Connect = GetUI_Button(nameof(btn_Connect));
        btn_CreateDummy = GetUI_Button(nameof(btn_CreateDummy), OnClick_CreateDummy);
        btn_DestroyDummy = GetUI_Button(nameof(btn_DestroyDummy), OnClick_DestroyDummy);
        btn_CreateMain = GetUI_Button(nameof(btn_CreateMain), OnClick_CreateMain);
        btn_DestroyMain = GetUI_Button(nameof(btn_DestroyMain), OnClick_DestroyMain);

        inputField_IpAddress.placeholder.GetComponent<TMP_Text>().text = "192.168.0.104";

        inputField_Port.placeholder.GetComponent<TMP_Text>().text = "7777";
    }

	private void OnClick_CreateMain()
	{
		var virtualCamrea = GameObject.Find("1").GetComponent<CinemachineVirtualCamera>();

		CinemachineSwitcher.SwitchCamera(virtualCamrea);

		var connectionId = inputField_ConnectionId.text;

		GameClientManager.Instance.CreateMain(connectionId);
	}

	private void OnClick_DestroyMain()
	{
        CinemachineSwitcher.SwitchMainCamera(CinemachineBlendDefinition.Style.EaseInOut);

        GameClientManager.Instance.DestroyMain();
	}

	private void OnClick_CreateDummy()
	{
		GameClientManager.Instance.CreateDummy();
	}

	private void OnClick_DestroyDummy()
    {
        GameClientManager.Instance.DestroyDummy();
    }
}
