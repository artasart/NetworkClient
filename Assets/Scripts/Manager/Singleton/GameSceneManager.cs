using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName
{
	Logo,
	Title,
	Login,
	Main
}

public class GameSceneManager : SingletonManager<GameSceneManager>
{
	#region Members

	GameObject go_MasterCanvas;

	public GameObject go_Fade { get; set; }
	public GameObject go_Dim { get; set; }

	CoroutineHandle handle_Show;
	CoroutineHandle handle_Fade;
	CoroutineHandle handle_Dim;

	bool isDone = false;

	#endregion



	#region Initialize

	private void Awake()
	{
		CacheTransUI();
	}

	private void CacheTransUI()
	{
		go_MasterCanvas = Instantiate(Resources.Load<GameObject>(Define.PATH_UI + "go_Canvas_Master"), Vector3.zero, Quaternion.identity, this.transform);
		go_MasterCanvas.name = "go_Canvas_Master";

		go_Fade = CreateTransUI(Resources.Load<GameObject>(Define.PATH_UI + "go_Fade"));
		go_Dim = CreateTransUI(Resources.Load<GameObject>(Define.PATH_UI + "go_Dim"));
	}

	private GameObject CreateTransUI(GameObject _prefab)
	{
		var element = Instantiate(_prefab, Vector3.zero, Quaternion.identity, go_MasterCanvas.transform);
		var rectTransform = element.GetComponent<RectTransform>();
		var canvasGroup = element.GetComponent<CanvasGroup>();

		rectTransform.localPosition = Vector3.zero;
		canvasGroup.alpha = 0f;
		canvasGroup.blocksRaycasts = false;

		element.SetActive(false);

		return element;
	}

	#endregion



	#region Core Methods

	public void LoadScene(SceneName _sceneName, bool _isAsync = true) => Timing.RunCoroutine(Co_LoadScene(_sceneName, _isAsync));

	private IEnumerator<float> Co_LoadScene(SceneName _sceneName, bool _isAsync = true)
	{
		Fade(true, .5f);

		yield return Timing.WaitUntilTrue(() => isDone);

		string sceneName = GetSceneName(_sceneName);

        if (_isAsync)
        {
            SceneManager.LoadSceneAsync(sceneName);
        }

        else
        {
			SceneManager.LoadScene(sceneName);
		}

		yield return Timing.WaitUntilTrue(() => IsSceneLoaded(sceneName));

		DebugManager.Log($"{_sceneName} is loaded.", DebugColor.Scene);
	}

	#endregion



	#region Basic Methods

	public void UnloadSceneAsync(string _sceneName)
	{
		SceneManager.UnloadSceneAsync(_sceneName);

		DebugManager.Log($"{_sceneName} is unloaded async.", DebugColor.Scene);
	}

    public bool IsSceneLoaded(string _sceneName)
    {
        return SceneManager.GetSceneByName(_sceneName).isLoaded;
    }

    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public int GetCurrentSceneBuildIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    public string GetSceneNameByBuildIndex(int _buildIndex)
    {
        Scene scene = SceneManager.GetSceneByBuildIndex(_buildIndex);

        return scene.name;
    }

	public string GetSceneName(SceneName _sceneName)
	{
		string sceneName = string.Empty;

		switch(_sceneName)
		{
			case SceneName.Logo:
				sceneName = "01_" + _sceneName.ToString();
				break;
			case SceneName.Title:
				sceneName = "02_" + _sceneName.ToString();
				break;
			case SceneName.Login:
				sceneName = "03_" + _sceneName.ToString();
				break;
			case SceneName.Main:
				sceneName = "04_" + _sceneName.ToString();
				break;
		}

		return sceneName;
	}

	#endregion



	#region Utils

	public void Fade(bool _isFade, float _fadeSpeed = 1f)
	{
		handle_Fade = Timing.RunCoroutine(Co_Transition(go_Fade, _isFade, _fadeSpeed), Define.FADE);

		DebugManager.Log($"Fade : {_isFade}", DebugColor.UI);
	}

	public void Dim(bool _isDim, float _dimSpeed = 1f)
	{
		Timing.KillCoroutines(handle_Dim);
		Timing.KillCoroutines(handle_Show);

		handle_Dim = Timing.RunCoroutine(Co_Transition(go_Dim, _isDim, _dimSpeed), Define.DIM);

		DebugManager.Log($"Dim : {_isDim}", DebugColor.UI);
	}

	private IEnumerator<float> Co_Transition(GameObject _meta, bool _isFade, float _lerpSpeed)
	{
		if (handle_Show.IsRunning) yield break;

		isDone = false;

		_meta.transform.SetAsLastSibling();
		_meta.SetActive(true);

		handle_Show = Timing.RunCoroutine(Co_Show(_meta, _isFade, _lerpSpeed), _meta.GetHashCode());

		yield return Timing.WaitUntilDone(handle_Show);

		if (!_isFade)
		{
			_meta.SetActive(false);
		}

		isDone = true;
	}

	public bool IsFaded()
	{
		return isDone;
	}

	private IEnumerator<float> Co_Show(GameObject _gameObject, bool _isShow, float _lerpSpeed = 1f)
	{
		var canvasGroup = _gameObject.GetComponent<CanvasGroup>();
		var targetAlpha = _isShow ? 1f : 0f;
		var lerpvalue = 0f;
		var lerpspeed = _lerpSpeed;

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

	public void FadeInstant(bool _enable)
	{
		go_Fade.SetActive(_enable);
		go_Fade.GetComponent<CanvasGroup>().alpha = _enable ? 1f : 0f;
		go_Fade.GetComponent<CanvasGroup>().blocksRaycasts = _enable;
	}

	#endregion
}
