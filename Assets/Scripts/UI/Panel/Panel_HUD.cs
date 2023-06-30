using UnityEngine;
using UnityEngine.UI;

public class Panel_HUD : Panel_Base
{
    Button btn_Casino;

	protected override void Awake()
	{
		base.Awake();

		btn_Casino = GetUI_Button(nameof(btn_Casino), () =>
		{
			GameManager.UI.StackPanel<Panel_Casino>();
		});
	}
}