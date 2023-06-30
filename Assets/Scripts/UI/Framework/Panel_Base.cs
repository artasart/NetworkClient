using System.Linq;
using UnityEngine.UI;

public class Panel_Base : UI_Base
{
	protected override void Awake()
	{
		base.Awake();
				
		CloseTabAll();
	}


	protected void ChangeTab(string _hierarchyName)
	{
		CloseTabAll();

		childUI[_hierarchyName].SetActive(true);
	}

	private void CloseTab(string _hierarchyName)
	{
		childUI[_hierarchyName].SetActive(false);
	}

	private void CloseTabAll()
	{
		childUI.Values
			.Where(uiObject => uiObject.GetComponent<Tab_Base>() != null)
			.ToList()
			.ForEach(tabObject => tabObject.SetActive(false));
	}
}