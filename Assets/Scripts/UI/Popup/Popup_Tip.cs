using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Tip : Popup_Base
{
	Button btn_Close;

	protected override void Awake()
	{
		base.Awake();

		btn_Close = GetUI_Button(nameof(btn_Close), () => GameManager.UI.PopPopup(true));
	}
}
