using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Panel_GameStart : Panel_Base
{
	Button btn_GameStart;
	TMP_Text txtmp_GameStart;

	protected override void Awake()
	{
		base.Awake();

		btn_GameStart = GetUI_Button(nameof(btn_GameStart), OnClick_GameStart);
		txtmp_GameStart = GetUI_TMPText(nameof(txtmp_GameStart), "PRESS TO START");
	}

	private void OnClick_GameStart()
	{
		GameManager.UI.StackPopup<Popup_Login>();

		GameManager.UI.FadeMaskableGrahpic(txtmp_GameStart, 0f);
	}

	public void GameStart(bool _isStart)
	{
		GameManager.UI.FadeMaskableGrahpic(txtmp_GameStart, _isStart ? 0f : 1f);
	}
}
