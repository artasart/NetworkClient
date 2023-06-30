using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Base : MonoBehaviour
{
	public Dictionary<string, GameObject> childUI = new Dictionary<string, GameObject>();



	#region Initialize

	protected virtual void Awake()
	{
		FindAllChildUI();
	}

	public void FindAllChildUI() => SearchUI(this.transform);

	private void SearchUI(Transform _parent)
	{
		foreach (Transform child in _parent)
		{
			Tab_Base tab = child.GetComponent<Tab_Base>();

			if (tab != null)
			{
				if (child.parent == this.transform)
				{
					childUI[child.name] = child.gameObject;
				}
			}

			else
			{
				childUI[child.name] = child.gameObject;

				SearchUI(child);
			}
		}
	}

	#endregion



	#region Basic Methods

	protected TMP_Text GetUI_TMPText(string _hierarchyName, string _message)
	{
		if (childUI.ContainsKey(_hierarchyName))
		{
			var txtmp = childUI[_hierarchyName].GetComponent<TMP_Text>();
			txtmp.text = _message;

			return txtmp;
		}

		else { Debug.Log($"WARNING : {_hierarchyName} is not in this hierarchy."); return null; }
	}

	protected Button GetUI_Button(string _hierarchyName, Action _action = null)
	{
		if (childUI.ContainsKey(_hierarchyName))
		{
			var button = childUI[_hierarchyName].GetComponent<Button>();

			button.onClick.AddListener(() => GameManager.Sound.PlaySoundEffect("Click_2"));

			button.onClick.AddListener(() => _action?.Invoke());

			return button;
		}

		else { Debug.Log($"WARNING : {_hierarchyName} is not in this hierarchy."); return null; }
	}

	#endregion
}
