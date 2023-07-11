using UnityEngine;
using UnityEngine.UI;

public class Panel_HUD : Panel_Base
{
    Button btn_Setting;
    Button btn_Store;
    Button btn_Tip;
    Button btn_Test;

	protected override void Awake()
	{
		base.Awake();

		btn_Tip = GetUI_Button(nameof(btn_Tip), OnClick_Tip);
		btn_Store = GetUI_Button(nameof(btn_Store), OnClick_Store);
		btn_Setting = GetUI_Button(nameof(btn_Setting), OnClick_Setting);
		btn_Test = GetUI_Button(nameof(btn_Test), OnClick_Test);
	}

	private void OnClick_Setting()
	{
		GameManager.UI.StackPopup<Popup_Setting>(true);
	}

	private void OnClick_Tip()
	{
		FindObjectOfType<MonsterGenerator>().Generate(1);
	}

	private void OnClick_Store()
	{
		FindObjectOfType<MonsterGenerator>().Generate(20f);
	}

	private void OnClick_Test()
	{
		var monsters = FindObjectsOfType<MonsterActor>();

		for (int i = 0; i < monsters.Length; i++)
		{
			monsters[i].Die();
		}
	}
}