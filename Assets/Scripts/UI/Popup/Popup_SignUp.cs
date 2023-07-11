using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup_SignUp : Popup_Base
{
	Button btn_Close;

	TMP_InputField inputField_Email;
	TMP_InputField inputField_Username;
	TMP_InputField inputField_Password;

	Button btn_SignUp;

	protected override void Awake()
	{
		base.Awake();

		btn_Close = GetUI_Button(nameof(btn_Close), OnClick_Close);
		btn_SignUp = GetUI_Button(nameof(btn_SignUp), OnClick_SignUp);

		inputField_Email = GetUI_TMPInputField(nameof(inputField_Email), OnValueChange_Email);
		inputField_Username = GetUI_TMPInputField(nameof(inputField_Username), OnValueChange_UserName);
		inputField_Password = GetUI_TMPInputField(nameof(inputField_Password), OnValueChange_Password);
	}

	public void OnClick_SignUp()
	{
		GameManager.Login.SignUp(inputField_Username.text, inputField_Password.text);
	}

	private void OnValueChange_UserName(string _value)
	{

	}

	private void OnValueChange_Password(string _value)
	{

	}

	private void OnValueChange_Email(string _value)
	{

	}
}
