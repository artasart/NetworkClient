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

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Back();
		}
	}

	public void Back()
	{
		GameManager.Sound.PlaySound("Click_1");

		if (openPopups.Count > 0)
		{
			PopPopup(true);
		}

		else if (openPanels.Count > 0)
		{
			PopPanel();
		}
	}

	#endregion



	#region Core Methods

	public T GetPanel<T>() where T : Component
	{
		if (!panels.ContainsKey(typeof(T).ToString())) return null;

		return panels[typeof(T).ToString()].GetComponent<T>();
	}

	public void StackPanel<T>(bool _instant = false) where T : Component
	{
		string panelName = typeof(T).ToString();

		if (openPanels.Contains(panelName)) return;

		openPanels.Push(panelName);

		panels[panelName].transform.SetAsLastSibling();

		if (_instant)
		{
			panels[panelName].SetActive(true);
			panels[panelName].GetComponent<CanvasGroup>().alpha = 1f;
			panels[panelName].GetComponent<CanvasGroup>().blocksRaycasts = true;
		}

		else ShowPanel(panels[panelName], true);

		DebugManager.Log($"Push: {panelName}", DebugColor.UI);
	}

	public void PopPanel(bool _instant = false)
	{
		if (openPanels.Count <= 0) return;

		var panelName = openPanels.Pop();

		if (_instant)
		{
			panels[panelName].SetActive(false);
			panels[panelName].GetComponent<CanvasGroup>().alpha = 0f;
			panels[panelName].GetComponent<CanvasGroup>().blocksRaycasts = false;
		}

		else ShowPanel(panels[panelName], false);

		DebugManager.Log($"Pop: {panelName}", DebugColor.UI);
	}

	public void PopPanelAll(bool _instant = false)
	{
		while (openPanels.Count > 0)
		{
			var panelName = openPanels.Pop();

			if (_instant)
			{
				panels[panelName].SetActive(false);
				panels[panelName].GetComponent<CanvasGroup>().alpha = 0f;
				panels[panelName].GetComponent<CanvasGroup>().blocksRaycasts = false;
			}

			else ShowPanel(panels[panelName], false);

			DebugManager.Log($"Pop: {panelName}", DebugColor.UI);
		}
	}



	public T GetPopup<T>() where T : Component
	{
		if (!popups.ContainsKey(typeof(T).ToString())) return null;

		return popups[typeof(T).ToString()].GetComponent<T>();
	}

	public void PushPopup<T>(bool _instant = false) where T : MonoBehaviour
	{
		string popupName = typeof(T).Name;

		if (openPopups.Contains(popupName)) return;

		openPopups.Push(popupName);

		popups[popupName].transform.SetAsLastSibling();

		if (_instant)
		{
			popups[popupName].SetActive(true);
			popups[popupName].GetComponent<CanvasGroup>().alpha = 1f;
			popups[popupName].GetComponent<CanvasGroup>().blocksRaycasts = true;
		}

		else ShowPanel(popups[popupName], true);

		DebugManager.Log($"Push: {popupName}", DebugColor.UI);
	}

	public void PopPopup(bool _instant = false)
	{
		if (openPopups.Count <= 0) return;

		var popupName = openPopups.Pop();

		if (_instant)
		{
			popups[popupName].SetActive(false);
			popups[popupName].GetComponent<CanvasGroup>().alpha = 0f;
			popups[popupName].GetComponent<CanvasGroup>().blocksRaycasts = false;
		}

		else ShowPanel(popups[popupName], false);

		DebugManager.Log($"Pop: {popupName}", DebugColor.UI);
	}

	public void PopPopupAll(bool _instant = false)
	{
		while (openPopups.Count > 0)
		{
			var popupName = openPopups.Pop();

			if (_instant)
			{
				popups[popupName].SetActive(false);
				popups[popupName].GetComponent<CanvasGroup>().alpha = 0f;
				popups[popupName].GetComponent<CanvasGroup>().blocksRaycasts = false;
			}

			else ShowPanel(popups[popupName], false);

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

	public void Show(GameObject _gameObject, bool _isShow)
	{
		Timing.RunCoroutine(Co_Show(_gameObject, _isShow), _gameObject.GetHashCode());
	}

	public void ShowButton(GameObject _gameObject, float _target, float _lerpspeed = 1f)
	{
		Timing.RunCoroutine(Co_ShowButton(_gameObject, _target, _lerpspeed), _gameObject.GetHashCode());
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

	private IEnumerator<float> Co_ShowButton(GameObject _gameObject, float _target, float _lerpspeed = 1f)
	{
		var canvasGroup = _gameObject.GetComponent<CanvasGroup>();
		var targetAlpha = _target;
		var lerpvalue = 0f;
		var lerpspeed = _lerpspeed;

		if (_target != 1) canvasGroup.blocksRaycasts = false;

		while (Mathf.Abs(canvasGroup.alpha - targetAlpha) >= 0.001f)
		{
			canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, lerpvalue += lerpspeed * Time.deltaTime);

			yield return Timing.WaitForOneFrame;
		}

		if (_target == 1) canvasGroup.blocksRaycasts = true;

		canvasGroup.alpha = targetAlpha;
	}

	#endregion
}