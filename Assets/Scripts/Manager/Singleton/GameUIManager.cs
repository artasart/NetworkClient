using MEC;
using System.Collections.Generic;
using UnityEngine;
using static EasingFunction;

public class GameUIManager : SingletonManager<GameUIManager>
{
	#region Members

	[NonReorderable] Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
	[NonReorderable] Dictionary<string, GameObject> popups = new Dictionary<string, GameObject>();

	Stack<string> openPanels = new Stack<string>();
	Stack<string> openPopups = new Stack<string>();

	GameObject group_MasterCanvas;
	GameObject group_Panel;
	GameObject group_Popup;

	public Canvas MasterCanvas { get => group_MasterCanvas.GetComponent<Canvas>(); }

	#endregion



	#region Initialize

	private void Awake()
	{
		group_MasterCanvas = GameObject.Find("go_Canvas");

		group_Panel = GameObject.Find(nameof(group_Panel));
		group_Popup = GameObject.Find(nameof(group_Popup));

		CacheUI(group_Panel, panels);
		CacheUI(group_Popup, popups);
	}

	private void CacheUI(GameObject _parent, Dictionary<string, GameObject> _objects)
	{
		for (int i = 0; i < _parent.transform.childCount; i++)
		{
			var child = _parent.transform.GetChild(i);
			var name = child.name;

			if (_objects.ContainsKey(name))
			{
				DebugManager.Log($"Same Key is registered in {_parent.name}", DebugColor.UI);
				continue;
			}

			child.gameObject.SetActive(true);
			child.gameObject.GetComponent<CanvasGroup>().alpha = 0f;
			child.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
			child.gameObject.SetActive(false);

			_objects[name] = child.gameObject;
		}
	}

	#endregion



	#region Core Methods

	public void StackPanel<T>() where T : Component
	{
		string panelName = typeof(T).ToString();

		if (openPanels.Contains(panelName)) return;

		openPanels.Push(panelName);

		panels[panelName].transform.SetAsLastSibling();

		ShowPanel(panels[panelName], true);

		DebugManager.Log($"Push: {panelName}", DebugColor.UI);
	}

	public void PopPanel()
	{
		if (openPanels.Count <= 0) return;

		var panelName = openPanels.Pop();

		ShowPanel(panels[panelName], false);

		DebugManager.Log($"Pop: {panelName}", DebugColor.UI);
	}

	public void PopPanelAll()
	{
		while (openPanels.Count > 0)
		{
			var panelName = openPanels.Pop();

			ShowPanel(panels[panelName], false);

			DebugManager.Log($"Pop: {panelName}", DebugColor.UI);
		}
	}

	public void SwitchPanel(string _panelName)
	{
		if (openPanels.Count > 0)
		{
			var prevPanelName = openPanels.Peek();

			if (prevPanelName == _panelName)
			{
				DebugManager.Log($"Panel {_panelName} is already open.", DebugColor.UI);
				return;
			}

			ShowPanel(panels[prevPanelName], false);

			openPanels.Pop();
		}

		openPanels.Push(_panelName);

		panels[_panelName].transform.SetAsLastSibling();

		ShowPanel(panels[_panelName], true);

		DebugManager.Log($"Switched: {_panelName}", DebugColor.UI);
	}

	public GameObject GetPanel(string _panelName)
	{
		if (!panels.ContainsKey(_panelName)) return null;

		return panels[_panelName];
	}

	public T GetPanel<T>() where T : Component
	{
		if (!panels.ContainsKey(typeof(T).ToString())) return null;

		return panels[typeof(T).ToString()].GetComponent<T>();
	}


	public void PushPopup(string _popupName)
	{
		if (openPopups.Contains(_popupName)) return;

		openPopups.Push(_popupName);

		popups[_popupName].transform.SetAsLastSibling();

		ShowPopup(popups[_popupName], true);

		DebugManager.Log($"Push: {_popupName}", DebugColor.UI);
	}

	public void PushPopup<T>() where T : MonoBehaviour
	{
		string popupName = typeof(T).Name;

		if (openPopups.Contains(popupName)) return;

		openPopups.Push(popupName);

		popups[popupName].transform.SetAsLastSibling();

		ShowPopup(popups[popupName], true);

		DebugManager.Log($"Push: {popupName}", DebugColor.UI);
	}

	public void PopPopup()
	{
		if (openPopups.Count <= 0) return;

		var popupName = openPopups.Pop();

		ShowPopup(popups[popupName], false);

		DebugManager.Log($"Pop: {popupName}", DebugColor.UI);
	}

	public void PopPopupAll()
	{
		while (openPopups.Count > 0)
		{
			var popupName = openPopups.Pop();

			ShowPopup(popups[popupName], false);

			DebugManager.Log($"Pop: {popupName}", DebugColor.UI);
		}
	}


	#endregion



	#region Basic Methods

	public void ShowPanel(GameObject _gameObject, bool _isShow)
	{
		Timing.RunCoroutine(Co_Show(_gameObject, _isShow, 1.5f), _gameObject.GetHashCode());
	}

	public void ShowPopup(GameObject _gameObject, bool _isShow)
	{
		float delay = _isShow ? 0f : .1f;
		float showDelay = _isShow ? 0f : .175f;

		Timing.RunCoroutine(Co_Show(_gameObject, _isShow).Delay(delay + showDelay), _gameObject.GetHashCode());
		Timing.RunCoroutine(Co_Ease(_gameObject, _isShow).Delay(.1f - delay), _gameObject.GetHashCode());
	}

	private IEnumerator<float> Co_Show(GameObject _gameObject, bool _isShow, float _lerpspeed = 1f)
	{
		var canvasGroup = _gameObject.GetComponent<CanvasGroup>();
		var targetAlpha = _isShow ? 1f : 0f;
		var lerpvalue = 0f;
		var lerpspeed = _lerpspeed;

		if (!_isShow) canvasGroup.blocksRaycasts = false;
		else _gameObject.SetActive(true);

		while (Mathf.Abs(canvasGroup.alpha - targetAlpha) >= 0.001f)
		{
			canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, lerpvalue += lerpspeed * Time.deltaTime);

			yield return Timing.WaitForOneFrame;
		}

		canvasGroup.alpha = targetAlpha;

		if (_isShow) canvasGroup.blocksRaycasts = true;
		else _gameObject.SetActive(false);
	}

	private IEnumerator<float> Co_Ease(GameObject _gameObject, bool _show, float _lerpspeed = 1f)
	{
		GameObject group_Modal = _gameObject.transform.Search(nameof(group_Modal))?.gameObject;

		if (group_Modal == null) yield break;

		float current = group_Modal.GetComponent<RectTransform>().localScale.x;
		float target = _show ? 1f : 0f;
		var fucntion = _show ? Ease.EaseOutBack : Ease.EaseInBack;

		float lerpvalue = 0f;
		float lerpspeed = _show ? _lerpspeed * 3f : 1.5f;

		group_Modal.GetComponent<RectTransform>().localScale = Vector3.one * Mathf.Abs(1 - target);

		while (lerpvalue <= 1f)
		{
			Function function = GetEasingFunction(fucntion);

			float x = function(current, target, lerpvalue);
			float y = function(current, target, lerpvalue);
			float z = function(current, target, lerpvalue);

			group_Modal.GetComponent<RectTransform>().localScale = new Vector3(x, y, z);

			lerpvalue += 3f * Time.deltaTime;

			yield return Timing.WaitForOneFrame;
		}

		group_Modal.GetComponent<RectTransform>().localScale = Vector3.one * target;
	}

	#endregion
}