using UnityEngine;
using UnityEngine.UI;

public class Panel_HUD : Panel_Base
{
    Button btn_Setting;
    Button btn_Store;
    Button btn_Tip;

	protected override void Awake()
	{
		base.Awake();

		btn_Tip = GetUI_Button(nameof(btn_Tip), OnClick_Tip);
		btn_Store = GetUI_Button(nameof(btn_Store), OnClick_Store);
		btn_Setting = GetUI_Button(nameof(btn_Setting), OnClick_Setting);
	}

	private void OnClick_Setting()
	{
		GameManager.UI.PushPopup<Popup_Setting>(true);
	}

	private void OnClick_Tip()
	{
		GameManager.UI.PushPopup<Popup_Tip>(true);
	}

	private void OnClick_Store()
	{
		GameManager.UI.PushPopup<Popup_Store>(true);
	}
}