using UnityEngine;
using UnityEngine.UI;

public enum ModalType
{
	Confrim,
	ConfirmCancel,
}

public class Popup_Basic : Popup_Base
{
	Button btn_Confirm;
	Button btn_Close;
	Button btn_Background;

	public ModalType modalType = ModalType.ConfirmCancel;

	private void OnEnable()
	{
		switch(modalType)
		{
			case ModalType.Confrim:
				btn_Confirm.gameObject.SetActive(false);
				btn_Close.gameObject.SetActive(true);
				break;
			case ModalType.ConfirmCancel:
				btn_Confirm.gameObject.SetActive(true);
				btn_Confirm.gameObject.SetActive(true);
				break;
		}
	}

	protected override void Awake()
	{
		base.Awake();

		btn_Confirm = GetUI_Button(nameof(btn_Confirm), OnClick_Confirm);
		btn_Close = GetUI_Button(nameof(btn_Close), OnClick_Close);
		btn_Background = GetUI_Button(nameof(btn_Background), OnClick_Close);
	}
}