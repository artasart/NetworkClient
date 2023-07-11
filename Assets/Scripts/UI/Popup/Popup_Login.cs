using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Popup_Login : Popup_Base
{
	Button btn_Close;

	Button btn_Login;
	Button btn_SignUp;

	TMP_InputField inputField_Username;
	TMP_InputField inputField_Password;

	protected override void Awake()
	{
		base.Awake();

		btn_Login = GetUI_Button(nameof(btn_Login), OnClick_Login);
		btn_SignUp = GetUI_Button(nameof(btn_SignUp), OnClick_SignUp);
		btn_Close = GetUI_Button(nameof(btn_Close), OnClick_Close);

		inputField_Username = GetUI_TMPInputField(nameof(inputField_Username), OnValueChange_UserName);
		inputField_Password = GetUI_TMPInputField(nameof(inputField_Password), OnValueChange_Password);
	}

	private void Start()
	{
		GameManager.Login.selectables.Add(inputField_Username);
		GameManager.Login.selectables.Add(inputField_Password);
		GameManager.Login.selectables.Add(btn_Login);
	}

	private void OnClick_Login()
	{
		GameManager.Login.Login(inputField_Username.text, inputField_Password.text);
	}

	private void OnClick_SignUp()
	{
		GameManager.UI.StackPopup<Popup_SignUp>();
	}

	private void OnValueChange_UserName(string _value)
	{

	}

	private void OnValueChange_Password(string _value)
	{

	}

	protected override void OnClick_Close()
	{
		base.OnClick_Close();

		GameManager.UI.GetPanel<Panel_GameStart>().GameStart(false);
	}
}
