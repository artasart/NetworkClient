using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Setting : Popup_Base
{
	Button btn_Setting;
	Button btn_Support;
	Button btn_Close;

	protected override void Awake()
	{
		base.Awake();

		btn_Setting = GetUI_Button(nameof(btn_Setting), () => ChangeTab<Tab_Setting>());
		btn_Support = GetUI_Button(nameof(btn_Support), () => ChangeTab<Tab_Support>());
		btn_Close = GetUI_Button(nameof(btn_Close), () => GameManager.UI.PopPopup(true));
	}

	private void Start()
	{
		ChangeTab<Tab_Setting>();
	}

	public void Test()
	{
		Debug.Log("Test");
	}
}
