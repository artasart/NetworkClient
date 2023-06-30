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
	Button btn_Cancel;
	Button btn_Background;

	public ModalType modalType = ModalType.ConfirmCancel;

	private void OnEnable()
	{
		switch(modalType)
		{
			case ModalType.Confrim:
				btn_Confirm.gameObject.SetActive(false);
				btn_Cancel.gameObject.SetActive(true);
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
		btn_Cancel = GetUI_Button(nameof(btn_Cancel), OnClick_Cancel);
		btn_Background = GetUI_Button(nameof(btn_Background), OnClick_Cancel);
	}
}